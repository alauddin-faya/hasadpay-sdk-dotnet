using HasadPay.Net.Services;

namespace HasadPay.Net;

/// <summary>
/// Main HasadPay API client interface.
/// </summary>
public interface IHasadPayClient
{
    /// <summary>
    /// Configuration options for this client instance.
    /// </summary>
    HasadPayOptions Options { get; }

    /// <summary>
    /// Service for creating, confirming, and inspecting payment transactions.
    /// </summary>
    ITransactionsService Transactions { get; }

    /// <summary>
    /// Service for creating, retrieving, and cancelling payment invoices.
    /// </summary>
    IInvoicesService Invoices { get; }

    /// <summary>
    /// Service for discovering available payment methods and rails.
    /// </summary>
    IPaymentMethodsService Methods { get; }

    /// <summary>
    /// Executes an authenticated HTTP request against the HasadPay API.
    /// </summary>
    Task<TResponse> SendRequestAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body = null,
        CancellationToken cancellationToken = default);
}
