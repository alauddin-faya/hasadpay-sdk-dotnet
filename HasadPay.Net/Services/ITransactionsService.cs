using HasadPay.Net.Models.Transactions;

namespace HasadPay.Net.Services;

/// <summary>
/// Handles creation, confirmation, and status retrieval of HasadPay payment transactions.
/// </summary>
public interface ITransactionsService
{
    /// <summary>
    /// Creates a payment transaction session.
    /// 
    /// Way 1 (Hosted Multi-Rail Checkout):
    /// Leave request.Service null. Customer will be redirected to the hosted checkout screen.
    /// 
    /// Way 2 (Direct Rail Payment):
    /// Set request.Service to a specific provider code (e.g. PaymentBrand.Floosak).
    /// </summary>
    Task<TransactionResponse> CreateAsync(
        TransactionCreateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits OTP / PIN verification code to confirm a pending transaction.
    /// </summary>
    Task<TransactionResponse> ConfirmAsync(
        string transactionId,
        string otp,
        string? pin = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves real-time status and details of a transaction by ID or UUID.
    /// </summary>
    Task<TransactionResponse> GetAsync(
        string transactionId,
        CancellationToken cancellationToken = default);
}
