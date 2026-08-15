using System.Text.Json;
using HasadPay.Net.Models.Transactions;

namespace HasadPay.Net.Services;

/// <summary>
/// Implementation of HasadPay Transactions Service.
/// </summary>
public class TransactionsService : ITransactionsService
{
    private readonly IHasadPayClient _client;

    public TransactionsService(IHasadPayClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc/>
    public async Task<TransactionResponse> CreateAsync(
        TransactionCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Currency) && !string.IsNullOrWhiteSpace(_client.Options.DefaultCurrency))
        {
            request.Currency = _client.Options.DefaultCurrency;
        }

        if (string.IsNullOrWhiteSpace(request.EntityId) && !string.IsNullOrWhiteSpace(_client.Options.EntityId))
        {
            request.EntityId = _client.Options.EntityId;
        }

        var response = await _client.SendRequestAsync<TransactionResponse>(
            HttpMethod.Post,
            "api/v1/transactions/",
            request,
            cancellationToken);

        NormalizeUrls(response);
        return response;
    }

    /// <inheritdoc/>
    public async Task<TransactionResponse> ConfirmAsync(
        string transactionId,
        string otp,
        string? pin = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("Transaction ID cannot be null or empty.", nameof(transactionId));
        if (string.IsNullOrWhiteSpace(otp)) throw new ArgumentException("OTP cannot be null or empty.", nameof(otp));

        var payload = new TransactionConfirmRequest
        {
            Otp = otp.Trim(),
            Pin = pin?.Trim()
        };

        var response = await _client.SendRequestAsync<TransactionResponse>(
            HttpMethod.Post,
            $"api/v1/transactions/{Uri.EscapeDataString(transactionId)}/confirm/",
            payload,
            cancellationToken);

        NormalizeUrls(response);
        return response;
    }

    /// <inheritdoc/>
    public async Task<TransactionResponse> GetAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("Transaction ID cannot be null or empty.", nameof(transactionId));

        var response = await _client.SendRequestAsync<TransactionResponse>(
            HttpMethod.Get,
            $"api/v1/transactions/{Uri.EscapeDataString(transactionId)}/",
            null,
            cancellationToken);

        NormalizeUrls(response);
        return response;
    }

    private void NormalizeUrls(TransactionResponse response)
    {
        string baseUrl = _client.Options.BaseUrl.TrimEnd('/');

        if (!string.IsNullOrEmpty(response.RedirectUrl) && response.RedirectUrl.StartsWith('/'))
        {
            response.RedirectUrl = baseUrl + response.RedirectUrl;
        }

        if (!string.IsNullOrEmpty(response.CheckoutUrl) && response.CheckoutUrl.StartsWith('/'))
        {
            response.CheckoutUrl = baseUrl + response.CheckoutUrl;
        }

        if (string.IsNullOrEmpty(response.CheckoutUrl) && !string.IsNullOrEmpty(response.RedirectUrl))
        {
            response.CheckoutUrl = response.RedirectUrl;
        }
        else if (string.IsNullOrEmpty(response.RedirectUrl) && !string.IsNullOrEmpty(response.CheckoutUrl))
        {
            response.RedirectUrl = response.CheckoutUrl;
        }

        // Fallback for invoice / UUID
        if (string.IsNullOrEmpty(response.CheckoutUrl))
        {
            if (!string.IsNullOrEmpty(response.InvoiceNumber))
            {
                response.CheckoutUrl = $"{baseUrl}/checkout/?invoice_id={response.InvoiceNumber}";
                response.RedirectUrl = response.CheckoutUrl;
            }
            else if (!string.IsNullOrEmpty(response.Uuid))
            {
                response.CheckoutUrl = $"{baseUrl}/checkout/?id={response.Uuid}";
                response.RedirectUrl = response.CheckoutUrl;
            }
        }
    }
}
