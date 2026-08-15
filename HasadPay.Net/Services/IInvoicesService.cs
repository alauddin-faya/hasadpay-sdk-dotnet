using HasadPay.Net.Models.Invoices;

namespace HasadPay.Net.Services;

/// <summary>
/// Handles creation, inquiry, and cancellation of payment invoices.
/// </summary>
public interface IInvoicesService
{
    /// <summary>
    /// Creates a payment invoice / bill.
    /// </summary>
    Task<InvoiceResponse> CreateAsync(
        InvoiceCreateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves invoice details by ID, UUID, or Invoice Number.
    /// </summary>
    Task<InvoiceResponse> GetAsync(
        string invoiceIdOrNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an active unpaid invoice.
    /// </summary>
    Task<InvoiceResponse> CancelAsync(
        string invoiceIdOrNumber,
        CancellationToken cancellationToken = default);
}
