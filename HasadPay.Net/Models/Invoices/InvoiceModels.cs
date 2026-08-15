using System.Text.Json;
using System.Text.Json.Serialization;
using HasadPay.Net.Models.Common;

namespace HasadPay.Net.Models.Invoices;

/// <summary>
/// Payload to create a payment invoice / bill in HasadPay.
/// </summary>
public class InvoiceCreateRequest
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0.00";

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "YER";

    [JsonPropertyName("customer_name")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("customer_mobile")]
    public string CustomerMobile { get; set; } = string.Empty;

    [JsonPropertyName("customer_email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CustomerEmail { get; set; }

    [JsonPropertyName("merchant_reference")]
    public string MerchantReference { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonPropertyName("allow_partial_payment")]
    public bool AllowPartialPayment { get; set; } = false;

    [JsonPropertyName("due_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DueDate { get; set; }

    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<OrderItem>? Items { get; set; }

    [JsonPropertyName("beneficiaries")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SplitBeneficiary>? Beneficiaries { get; set; }
}

/// <summary>
/// Strongly-typed response for an invoice created or retrieved from HasadPay.
/// </summary>
public class InvoiceResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("invoice_number")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [JsonPropertyName("merchant_reference")]
    public string MerchantReference { get; set; } = string.Empty;

    [JsonPropertyName("customer_name")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("customer_mobile")]
    public string CustomerMobile { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0.00";

    [JsonPropertyName("paid_amount")]
    public string PaidAmount { get; set; } = "0.00";

    [JsonPropertyName("remaining_amount")]
    public string RemainingAmount { get; set; } = "0.00";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "active";

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "YER";

    [JsonPropertyName("checkout_url")]
    public string? CheckoutUrl { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("allow_partial_payment")]
    public bool AllowPartialPayment { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? RawData { get; set; }

    /// <summary>
    /// Helper to check if the invoice is fully settled and paid.
    /// </summary>
    public bool IsPaid()
    {
        return Status.Equals("paid", StringComparison.OrdinalIgnoreCase)
            || (decimal.TryParse(RemainingAmount, out var rem) && rem <= 0m);
    }

    /// <summary>
    /// Helper to check if partial payments were received.
    /// </summary>
    public bool IsPartialPaid()
    {
        return Status.Equals("partial_paid", StringComparison.OrdinalIgnoreCase)
            || Status.Equals("partial", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Helper to check if the invoice is active and awaiting payment.
    /// </summary>
    public bool IsActive()
    {
        var st = Status.ToLowerInvariant();
        return st is "active" or "pending" or "unpaid" or "1";
    }
}
