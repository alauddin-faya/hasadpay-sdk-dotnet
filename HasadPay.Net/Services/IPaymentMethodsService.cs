using HasadPay.Net.Models.PaymentMethods;

namespace HasadPay.Net.Services;

/// <summary>
/// Service for querying active payment methods and rails supported by the gateway.
/// </summary>
public interface IPaymentMethodsService
{
    /// <summary>
    /// Retrieves a flat list of available payment methods.
    /// </summary>
    Task<List<PaymentMethodItem>> GetAvailableMethodsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves payment methods grouped into categories (Wallets, Banks, Cards).
    /// </summary>
    Task<List<PaymentCategory>> GetCategorizedMethodsAsync(
        CancellationToken cancellationToken = default);
}
