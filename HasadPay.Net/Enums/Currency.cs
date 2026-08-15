namespace HasadPay.Net.Enums;

/// <summary>
/// Supported ISO-4217 Currency Codes for HasadPay Transactions and Invoices.
/// </summary>
public enum Currency
{
    /// <summary>
    /// Yemeni Rial (Default)
    /// </summary>
    YER = 1,

    /// <summary>
    /// Saudi Riyal
    /// </summary>
    SAR = 2,

    /// <summary>
    /// US Dollar
    /// </summary>
    USD = 3,

    /// <summary>
    /// UAE Dirham
    /// </summary>
    AED = 4,

    /// <summary>
    /// Omani Rial
    /// </summary>
    OMR = 5,

    /// <summary>
    /// Kuwaiti Dinar
    /// </summary>
    KWD = 6,

    /// <summary>
    /// Qatari Riyal
    /// </summary>
    QAR = 7,

    /// <summary>
    /// Bahraini Dinar
    /// </summary>
    BHD = 8,

    /// <summary>
    /// Euro
    /// </summary>
    EUR = 9,

    /// <summary>
    /// Egyptian Pound
    /// </summary>
    EGP = 10,

    /// <summary>
    /// Jordanian Dinar
    /// </summary>
    JOD = 11,

    /// <summary>
    /// Turkish Lira
    /// </summary>
    TRY = 12,

    /// <summary>
    /// British Pound Sterling
    /// </summary>
    GBP = 13
}

/// <summary>
/// Helper extensions for Currency enum.
/// </summary>
public static class CurrencyExtensions
{
    /// <summary>
    /// Resolves a currency string or enum to its ISO 3-letter uppercase code.
    /// </summary>
    public static string ToCode(this Currency currency) => currency.ToString();

    /// <summary>
    /// Parses a string into a Currency enum, falling back to YER if invalid.
    /// </summary>
    public static Currency ParseOrDefault(string? code, Currency defaultCurrency = Currency.YER)
    {
        if (string.IsNullOrWhiteSpace(code)) return defaultCurrency;
        return Enum.TryParse<Currency>(code.Trim(), true, out var result) ? result : defaultCurrency;
    }
}
