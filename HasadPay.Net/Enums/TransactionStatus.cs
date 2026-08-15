namespace HasadPay.Net.Enums;

/// <summary>
/// Status states of a payment transaction or invoice in HasadPay.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction created and awaiting customer payment or OTP confirmation.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Transaction is currently processing with the provider/wallet.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Transaction successfully completed and funds captured.
    /// </summary>
    Success = 2,

    /// <summary>
    /// Transaction failed due to rejection, insufficient funds, or provider error.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Partial payment received for invoice or multi-part settlement.
    /// </summary>
    PartialSuccess = 4,

    /// <summary>
    /// Transaction was cancelled by merchant or user.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Payment session timed out before completion.
    /// </summary>
    Expired = 6
}
