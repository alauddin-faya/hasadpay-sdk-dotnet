namespace HasadPay.Net.Enums;

/// <summary>
/// Supported Payment Method / Brand Service Codes in HasadPay.
/// </summary>
public static class PaymentBrand
{
    // Electronic Wallets
    public const string Floosak = "FLOOSAK";
    public const string Cash = "CASH";
    public const string SabaCash = "SABACASH";
    public const string Jawali = "JAWALI";
    public const string WeNet = "WENET";
    public const string Pocket = "POCKET";
    public const string OneCash = "ONECASH";
    public const string Payeer = "PAYEER";
    public const string PerfectMoney = "PERFECTMONEY";
    public const string Jaib = "JAIB";

    // Banking & Direct Rails
    public const string Kuraimi = "KUR";
    public const string Tadhamon = "TADHAMON";
    public const string YemenKuwaitBank = "YKB";
    public const string InternationalBankOfYemen = "IBY";
    public const string CACBank = "CAC";
    public const string ShamilBank = "SHAMIL";

    // Cards & Global Gateways
    public const string Visa = "VISA";
    public const string MasterCard = "MASTER";
    public const string Mada = "MADA";
    public const string PayPal = "PAYPAL";
    public const string Stripe = "STRIPE";
}
