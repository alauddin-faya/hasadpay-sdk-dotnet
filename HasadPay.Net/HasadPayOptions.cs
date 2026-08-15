namespace HasadPay.Net;

/// <summary>
/// Configuration options for the HasadPay .NET SDK.
/// </summary>
public class HasadPayOptions
{
    /// <summary>
    /// The default configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "HasadPay";

    /// <summary>
    /// The base URL of the HasadPay payment gateway API.
    /// Default: "https://merchent-local.fintechsys.net"
    /// </summary>
    public string BaseUrl { get; set; } = "https://merchent-local.fintechsys.net";

    /// <summary>
    /// Static API Key for authentication (Alternative to Username/Password or BearerToken).
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Static Bearer Token for authentication.
    /// </summary>
    public string? BearerToken { get; set; }

    /// <summary>
    /// Merchant username for automatic JWT authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Merchant password for automatic JWT authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Merchant Entity ID (UUID) provided by HasadPay.
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Secret key used to verify incoming HMAC-SHA256 webhook signatures.
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Default 3-letter currency code (e.g. YER, SAR, USD).
    /// </summary>
    public string DefaultCurrency { get; set; } = "YER";

    /// <summary>
    /// HTTP request timeout in seconds. Default: 30.
    /// </summary>
    public double TimeoutSeconds { get; set; } = 30.0;

    /// <summary>
    /// Maximum number of automatic retries on transient network failures. Default: 2.
    /// </summary>
    public int MaxRetries { get; set; } = 2;
}
