using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HasadPay.Net.Exceptions;
using HasadPay.Net.Services;

namespace HasadPay.Net;

/// <summary>
/// Default implementation of the HasadPay API client.
/// </summary>
public class HasadPayClient : IHasadPayClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly SemaphoreSlim _authLock = new(1, 1);

    private string? _cachedJwtToken;
    private DateTimeOffset _jwtExpiresAt = DateTimeOffset.MinValue;

    public HasadPayOptions Options { get; }

    public ITransactionsService Transactions { get; }
    public IInvoicesService Invoices { get; }
    public IPaymentMethodsService Methods { get; }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString | System.Text.Json.Serialization.JsonNumberHandling.WriteAsString,
        Converters = { new HasadPay.Net.Converters.FlexibleStringConverter(), new HasadPay.Net.Converters.FlexibleIntConverter() }
    };

    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public HasadPayClient(Microsoft.Extensions.Options.IOptions<HasadPayOptions> options, HttpClient httpClient)
        : this(options?.Value ?? throw new ArgumentNullException(nameof(options)), httpClient)
    {
    }

    public HasadPayClient(HasadPayOptions options, HttpClient? httpClient = null)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));

        if (httpClient != null)
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _httpClient = new HttpClient();
            _ownsHttpClient = true;
        }

        if (_httpClient.BaseAddress == null && !string.IsNullOrWhiteSpace(Options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(Options.BaseUrl.TrimEnd('/') + "/");
        }

        _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(5, Options.TimeoutSeconds));

        Transactions = new TransactionsService(this);
        Invoices = new InvoicesService(this);
        Methods = new PaymentMethodsService(this);
    }

    /// <summary>
    /// Executes an authenticated HTTP request to the HasadPay API.
    /// </summary>
    public async Task<TResponse> SendRequestAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync<TResponse>(method, path, body, isRetryAfterAuth: false, cancellationToken);
    }

    private async Task<TResponse> ExecuteWithRetryAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body,
        bool isRetryAfterAuth,
        CancellationToken cancellationToken)
    {
        var requestUri = new Uri(path.TrimStart('/'), UriKind.Relative);
        using var request = new HttpRequestMessage(method, requestUri);

        // Configure default headers
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd("HasadPay-DotNet-SDK/1.0.0 (.NET)");

        if (!string.IsNullOrWhiteSpace(Options.EntityId))
        {
            request.Headers.TryAddWithoutValidation("X-Entity-ID", Options.EntityId);
        }

        // Apply Authentication
        await ApplyAuthenticationHeadersAsync(request, cancellationToken);

        // Serialize body if present
        if (body != null)
        {
            string jsonBody = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new HasadPayNetworkException($"Failed to reach HasadPay server at {Options.BaseUrl}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new HasadPayNetworkException($"HasadPay request timed out after {Options.TimeoutSeconds}s.", ex);
        }

        string rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        // Handle 401 Unauthorized for Auto-JWT refresh
        if (response.StatusCode == HttpStatusCode.Unauthorized && !isRetryAfterAuth && HasJwtCredentials())
        {
            InvalidateJwtToken();
            return await ExecuteWithRetryAsync<TResponse>(method, path, body, isRetryAfterAuth: true, cancellationToken);
        }

        if (!response.IsSuccessStatusCode)
        {
            HandleApiError(response.StatusCode, rawResponse);
        }

        try
        {
            if (typeof(TResponse) == typeof(string))
            {
                return (TResponse)(object)rawResponse;
            }

            using var doc = JsonDocument.Parse(rawResponse);
            var root = doc.RootElement;

            JsonElement targetElement = root;
            if (root.ValueKind == JsonValueKind.Object &&
                (typeof(TResponse) == typeof(Models.Transactions.TransactionResponse) ||
                 typeof(TResponse) == typeof(Models.Invoices.InvoiceResponse)))
            {
                if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
                {
                    targetElement = dataProp;
                }
            }

            var result = JsonSerializer.Deserialize<TResponse>(targetElement.GetRawText(), JsonOptions);
            if (result == null)
            {
                throw new HasadPayApiException("Received empty response payload from HasadPay.", (int)response.StatusCode, rawResponse: rawResponse);
            }
            return result;
        }
        catch (JsonException ex)
        {
            throw new HasadPayApiException($"Failed to parse JSON response: {ex.Message}", (int)response.StatusCode, rawResponse: rawResponse);
        }
    }

    private bool HasJwtCredentials() => !string.IsNullOrEmpty(Options.Username) && !string.IsNullOrEmpty(Options.Password);

    private void InvalidateJwtToken()
    {
        _cachedJwtToken = null;
        _jwtExpiresAt = DateTimeOffset.MinValue;
    }

    private async Task ApplyAuthenticationHeadersAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(Options.BearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Options.BearerToken);
            return;
        }

        if (!string.IsNullOrWhiteSpace(Options.ApiKey))
        {
            request.Headers.TryAddWithoutValidation("X-API-Key", Options.ApiKey);
            return;
        }

        if (HasJwtCredentials())
        {
            string token = await GetOrRefreshJwtTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<string> GetOrRefreshJwtTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_cachedJwtToken) && DateTimeOffset.UtcNow < _jwtExpiresAt)
        {
            return _cachedJwtToken;
        }

        await _authLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrEmpty(_cachedJwtToken) && DateTimeOffset.UtcNow < _jwtExpiresAt)
            {
                return _cachedJwtToken;
            }

            var authPayload = new
            {
                username = Options.Username,
                password = Options.Password
            };

            var endpoints = new[] { "api/v1/auth/", "api/v1/auth/login/", "api/v1/token/", "api/v1/users/login/" };
            string? token = null;

            foreach (var endpoint in endpoints)
            {
                try
                {
                    var authReq = new HttpRequestMessage(HttpMethod.Post, endpoint)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(authPayload, JsonOptions), Encoding.UTF8, "application/json")
                    };
                    authReq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var authRes = await _httpClient.SendAsync(authReq, cancellationToken);
                    if (authRes.IsSuccessStatusCode)
                    {
                        var authJson = await authRes.Content.ReadAsStringAsync(cancellationToken);
                        using var doc = JsonDocument.Parse(authJson);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
                        {
                            if (dataProp.TryGetProperty("access_token", out var datProp) ||
                                dataProp.TryGetProperty("token", out datProp) ||
                                dataProp.TryGetProperty("access", out datProp))
                            {
                                token = datProp.GetString();
                            }
                        }

                        if (string.IsNullOrEmpty(token) && (
                            root.TryGetProperty("access_token", out var accessProp) ||
                            root.TryGetProperty("access", out accessProp) ||
                            root.TryGetProperty("token", out accessProp)))
                        {
                            token = accessProp.GetString();
                        }

                        if (!string.IsNullOrEmpty(token))
                        {
                            break;
                        }
                    }
                }
                catch
                {
                    // Continue to next endpoint
                }
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new HasadPayAuthenticationException($"Could not authenticate with HasadPay using username '{Options.Username}'. Please check credentials.");
            }

            _cachedJwtToken = token;
            _jwtExpiresAt = DateTimeOffset.UtcNow.AddMinutes(50); // Cache for 50 minutes
            return token;
        }
        finally
        {
            _authLock.Release();
        }
    }

    private static void HandleApiError(HttpStatusCode statusCode, string rawBody)
    {
        string message = $"HasadPay API request failed with status {(int)statusCode} ({statusCode}).";
        string? errorCode = null;
        Dictionary<string, string[]>? validationErrors = null;

        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errProp))
            {
                message = errProp.GetString() ?? message;
            }
            else if (root.TryGetProperty("message", out var msgProp))
            {
                message = msgProp.GetString() ?? message;
            }
            else if (root.TryGetProperty("detail", out var detailProp))
            {
                message = detailProp.GetString() ?? message;
            }

            if (root.TryGetProperty("code", out var codeProp) || root.TryGetProperty("error_code", out codeProp))
            {
                errorCode = codeProp.GetString();
            }

            if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
            {
                validationErrors = new Dictionary<string, string[]>();
                foreach (var prop in errorsProp.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        var list = new List<string>();
                        foreach (var item in prop.Value.EnumerateArray())
                        {
                            list.Add(item.GetString() ?? string.Empty);
                        }
                        validationErrors[prop.Name] = list.ToArray();
                    }
                    else if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        validationErrors[prop.Name] = new[] { prop.Value.GetString() ?? string.Empty };
                    }
                }
            }
        }
        catch
        {
            // Fall back to default message if response isn't JSON
        }

        switch (statusCode)
        {
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                throw new HasadPayAuthenticationException(message, (int)statusCode, errorCode, rawBody);

            case HttpStatusCode.NotFound:
                throw new HasadPayNotFoundException(message, (int)statusCode, errorCode, rawBody);

            case HttpStatusCode.BadRequest:
            case HttpStatusCode.UnprocessableEntity:
                throw new HasadPayValidationException(message, validationErrors, (int)statusCode, errorCode, rawBody);

            default:
                throw new HasadPayApiException(message, (int)statusCode, errorCode, rawBody);
        }
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
        _authLock.Dispose();
    }
}
