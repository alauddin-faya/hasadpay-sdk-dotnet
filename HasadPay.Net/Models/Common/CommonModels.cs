using System.Text.Json.Serialization;

namespace HasadPay.Net.Models.Common;

/// <summary>
/// Represents an item in a checkout order or invoice line.
/// </summary>
public class OrderItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = "0.00";

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonPropertyName("sku")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sku { get; set; }

    public OrderItem() { }

    public OrderItem(string name, decimal price, int quantity = 1, string? description = null, string? sku = null)
    {
        Name = name;
        Price = price.ToString("F2");
        Quantity = quantity;
        Description = description;
        Sku = sku;
    }
}

/// <summary>
/// Customer information associated with a transaction or invoice.
/// </summary>
public class CustomerInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("mobile_number")]
    public string? MobileNumber { get; set; }

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonPropertyName("address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Address { get; set; }

    [JsonPropertyName("ip_address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IpAddress { get; set; }
}

/// <summary>
/// Represents a split settlement beneficiary in a marketplace payout.
/// </summary>
public class SplitBeneficiary
{
    [JsonPropertyName("beneficiary_id")]
    public string BeneficiaryId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0.00";

    [JsonPropertyName("fee_bearer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FeeBearer { get; set; }

    public SplitBeneficiary() { }

    public SplitBeneficiary(string beneficiaryId, decimal amount, string? feeBearer = null)
    {
        BeneficiaryId = beneficiaryId;
        Amount = amount.ToString("F2");
        FeeBearer = feeBearer;
    }
}
