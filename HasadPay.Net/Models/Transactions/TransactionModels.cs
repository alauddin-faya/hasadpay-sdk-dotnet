using System.Text.Json;
using System.Text.Json.Serialization;
using HasadPay.Net.Models.Common;

namespace HasadPay.Net.Models.Transactions;

/// <summary>
/// Payload to create a payment transaction session.
/// 
/// Way 1 (Hosted Multi-Rail Checkout):
/// Leave <see cref="Service"/> null/empty. Customer is redirected to hosted checkout page with all available rails.
/// 
/// Way 2 (Direct Rail Payment):
/// Set <see cref="Service"/> to a specific rail code (e.g. PaymentBrand.Floosak, PaymentBrand.Kuraimi).
/// </summary>
public class TransactionCreateRequest
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0.00";

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "YER";

    [JsonPropertyName("mobile_number")]
    public string MobileNumber { get; set; } = string.Empty;

    [JsonPropertyName("request_refrence")]
    public string RequestReference { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Address { get; set; }

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonPropertyName("return_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReturnUrl { get; set; }

    [JsonPropertyName("service")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Service { get; set; }

    [JsonPropertyName("transaction_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TransactionType { get; set; } = "RequestPayTransaction";

    [JsonPropertyName("entity_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EntityId { get; set; }

    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<OrderItem>? Items { get; set; }

    [JsonPropertyName("beneficiaries")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SplitBeneficiary>? Beneficiaries { get; set; }

    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Payload to confirm a pending transaction with OTP / verification code.
/// </summary>
public class TransactionConfirmRequest
{
    [JsonPropertyName("otp")]
    public string Otp { get; set; } = string.Empty;

    [JsonPropertyName("pin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Pin { get; set; }

    public TransactionConfirmRequest() { }
    public TransactionConfirmRequest(string otp) => Otp = otp;
}

/// <summary>
/// Strongly-typed response from HasadPay transaction operations.
/// </summary>
public class TransactionResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0.00";

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("status_code")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("status_display")]
    public string? StatusDisplay { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "YER";

    [JsonPropertyName("service")]
    public object? Service { get; set; }

    [JsonPropertyName("service_name")]
    public string? ServiceName { get; set; }

    [JsonPropertyName("mobile_number")]
    public string? MobileNumber { get; set; }

    [JsonPropertyName("request_refrence")]
    public string? RequestReference { get; set; }

    [JsonPropertyName("reference_number")]
    public string? ReferenceNumber { get; set; }

    [JsonPropertyName("redirect_url")]
    public string? RedirectUrl { get; set; }

    [JsonPropertyName("checkout_url")]
    public string? CheckoutUrl { get; set; }

    [JsonPropertyName("invoice_number")]
    public string? InvoiceNumber { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("fee")]
    public string? Fee { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? RawData { get; set; }

    /// <summary>
    /// Helper property to get the best URL for redirecting customer to payment checkout.
    /// </summary>
    [JsonIgnore]
    public string? PaymentUrl => !string.IsNullOrEmpty(CheckoutUrl) ? CheckoutUrl : RedirectUrl;

    /// <summary>
    /// Checks if the transaction was captured and completed successfully.
    /// </summary>
    public bool IsSuccessful()
    {
        var st = Status.ToLowerInvariant();
        var sc = StatusCode ?? string.Empty;
        return st is "2" or "4" or "success" or "completed" or "partial_success" or "paid"
            || sc is "000.000.000" or "000.100.110";
    }

    /// <summary>
    /// Checks if the transaction is pending customer authorization or OTP confirmation.
    /// </summary>
    public bool IsPending()
    {
        var st = Status.ToLowerInvariant();
        var sc = StatusCode ?? string.Empty;
        return st is "0" or "1" or "pending" or "in_progress"
            || sc is "000.200.000" or "000.200.100";
    }

    /// <summary>
    /// Checks if the transaction failed.
    /// </summary>
    public bool IsFailed() => !IsSuccessful() && !IsPending();
}
