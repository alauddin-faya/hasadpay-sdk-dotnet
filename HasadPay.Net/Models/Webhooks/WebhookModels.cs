using System.Text.Json;
using System.Text.Json.Serialization;

namespace HasadPay.Net.Models.Webhooks;

/// <summary>
/// Parsed HasadPay asynchronous webhook event notification.
/// </summary>
public class WebhookEvent
{
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("data")]
    public WebhookEventData Data { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? RawPayload { get; set; }

    /// <summary>
    /// Checks if this event indicates a successfully captured payment.
    /// </summary>
    public bool IsPaymentSucceeded()
    {
        var ev = Event.ToLowerInvariant();
        if (ev is "payment.succeeded" or "payment_completed" or "transaction.paid" or "transaction.completed")
            return true;

        return Data.IsSuccess();
    }

    /// <summary>
    /// Checks if this event indicates a failed payment.
    /// </summary>
    public bool IsPaymentFailed()
    {
        var ev = Event.ToLowerInvariant();
        if (ev is "payment.failed" or "transaction.failed" or "payment_failed")
            return true;

        return Data.IsFailed();
    }

    /// <summary>
    /// Checks if this event indicates an invoice has been paid.
    /// </summary>
    public bool IsInvoicePaid()
    {
        var ev = Event.ToLowerInvariant();
        return ev is "invoice.paid" or "invoice.completed"
            || (Data.InvoiceNumber != null && IsPaymentSucceeded());
    }
}

/// <summary>
/// Event payload details inside a webhook notification.
/// </summary>
public class WebhookEventData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }

    [JsonPropertyName("amount")]
    public string? Amount { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("status_code")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("status_display")]
    public string? StatusDisplay { get; set; }

    [JsonPropertyName("request_refrence")]
    public string? RequestReference { get; set; }

    [JsonPropertyName("reference_number")]
    public string? ReferenceNumber { get; set; }

    [JsonPropertyName("service")]
    public object? Service { get; set; }

    [JsonPropertyName("service_name")]
    public string? ServiceName { get; set; }

    [JsonPropertyName("invoice_number")]
    public string? InvoiceNumber { get; set; }

    [JsonPropertyName("customer_name")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("customer_mobile")]
    public string? CustomerMobile { get; set; }

    [JsonPropertyName("result")]
    public WebhookResult? Result { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }

    public bool IsSuccess()
    {
        var st = (Status ?? string.Empty).ToLowerInvariant();
        var sc = StatusCode ?? Result?.Code ?? string.Empty;
        return st is "2" or "4" or "success" or "completed" or "paid"
            || sc is "000.000.000" or "000.100.110";
    }

    public bool IsFailed()
    {
        var st = (Status ?? string.Empty).ToLowerInvariant();
        return st is "failed" or "3" or "cancelled" or "expired";
    }
}

/// <summary>
/// Result status descriptor inside webhook payloads.
/// </summary>
public class WebhookResult
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
