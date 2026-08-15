using HasadPay.Net.Models.Invoices;

namespace HasadPay.Net.Services;

/// <summary>
/// Implementation of HasadPay Invoices Service.
/// </summary>
public class InvoicesService : IInvoicesService
{
    private readonly IHasadPayClient _client;

    public InvoicesService(IHasadPayClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc/>
    public async Task<InvoiceResponse> CreateAsync(
        InvoiceCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Currency) && !string.IsNullOrWhiteSpace(_client.Options.DefaultCurrency))
        {
            request.Currency = _client.Options.DefaultCurrency;
        }

        var response = await _client.SendRequestAsync<InvoiceResponse>(
            HttpMethod.Post,
            "api/v1/invoices/",
            request,
            cancellationToken);

        NormalizeUrls(response);
        return response;
    }

    /// <inheritdoc/>
    public async Task<InvoiceResponse> GetAsync(
        string invoiceIdOrNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(invoiceIdOrNumber))
            throw new ArgumentException("Invoice identifier cannot be null or empty.", nameof(invoiceIdOrNumber));

        var response = await _client.SendRequestAsync<InvoiceResponse>(
            HttpMethod.Get,
            $"api/v1/invoices/{Uri.EscapeDataString(invoiceIdOrNumber)}/",
            null,
            cancellationToken);

        NormalizeUrls(response);
        return response;
    }

    /// <inheritdoc/>
    public async Task<InvoiceResponse> CancelAsync(
        string invoiceIdOrNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(invoiceIdOrNumber))
            throw new ArgumentException("Invoice identifier cannot be null or empty.", nameof(invoiceIdOrNumber));

        var response = await _client.SendRequestAsync<InvoiceResponse>(
            HttpMethod.Post,
            $"api/v1/invoices/{Uri.EscapeDataString(invoiceIdOrNumber)}/cancel/",
            new { },
            cancellationToken);

        NormalizeUrls(response);
        return response;
    }

    private void NormalizeUrls(InvoiceResponse response)
    {
        string baseUrl = _client.Options.BaseUrl.TrimEnd('/');

        if (!string.IsNullOrEmpty(response.CheckoutUrl) && response.CheckoutUrl.StartsWith('/'))
        {
            response.CheckoutUrl = baseUrl + response.CheckoutUrl;
        }
        else if (string.IsNullOrEmpty(response.CheckoutUrl) && !string.IsNullOrEmpty(response.InvoiceNumber))
        {
            response.CheckoutUrl = $"{baseUrl}/checkout/?invoice_id={response.InvoiceNumber}";
        }
    }
}
