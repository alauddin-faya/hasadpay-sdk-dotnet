using System.Text.Json.Serialization;

namespace HasadPay.Net.Models.PaymentMethods;

/// <summary>
/// Represents an active payment rail / method available in HasadPay.
/// </summary>
public class PaymentMethodItem
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("name_ar")]
    public string? NameAr { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = "wallets";

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("logo_url")]
    public string? LogoUrl { get; set; }

    [JsonPropertyName("badge")]
    public string? Badge { get; set; }

    [JsonPropertyName("badge_ar")]
    public string? BadgeAr { get; set; }

    [JsonPropertyName("requires_otp")]
    public bool RequiresOtp { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Represents a grouped category of payment methods (e.g. Wallets, Banks, Cards).
/// </summary>
public class PaymentCategory
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string Icon { get; set; } = "💳";
    public List<PaymentMethodItem> Methods { get; set; } = new();
}
