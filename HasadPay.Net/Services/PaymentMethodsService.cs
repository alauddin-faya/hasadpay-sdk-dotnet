using System.Text.Json;
using HasadPay.Net.Models.PaymentMethods;

namespace HasadPay.Net.Services;

/// <summary>
/// Implementation of PaymentMethodsService.
/// </summary>
public class PaymentMethodsService : IPaymentMethodsService
{
    private readonly IHasadPayClient _client;

    public PaymentMethodsService(IHasadPayClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <inheritdoc/>
    public async Task<List<PaymentMethodItem>> GetAvailableMethodsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var rawJson = await _client.SendRequestAsync<string>(
                HttpMethod.Get,
                "api/v1/payment-methods/",
                null,
                cancellationToken);

            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            JsonElement itemsElement = root;
            if (root.TryGetProperty("data", out var dataProp))
            {
                itemsElement = dataProp;
            }
            else if (root.TryGetProperty("results", out var resultsProp))
            {
                itemsElement = resultsProp;
            }

            if (itemsElement.ValueKind == JsonValueKind.Array)
            {
                var list = JsonSerializer.Deserialize<List<PaymentMethodItem>>(itemsElement.GetRawText(), HasadPayClient.JsonOptions);
                if (list != null && list.Count > 0)
                {
                    return list;
                }
            }
        }
        catch
        {
            // Fall back to default catalog
        }

        return GetFallbackMethods();
    }

    /// <inheritdoc/>
    public async Task<List<PaymentCategory>> GetCategorizedMethodsAsync(
        CancellationToken cancellationToken = default)
    {
        var methods = await GetAvailableMethodsAsync(cancellationToken);

        var walletCategory = new PaymentCategory
        {
            Key = "wallets",
            Title = "Electronic Wallets",
            TitleAr = "المحافظ الإلكترونية",
            Icon = "📱",
            Methods = methods.Where(m => m.Category.Equals("wallets", StringComparison.OrdinalIgnoreCase) || m.Category.Equals("wallet", StringComparison.OrdinalIgnoreCase)).ToList()
        };

        var bankCategory = new PaymentCategory
        {
            Key = "banks",
            Title = "Bank Accounts & Direct Rails",
            TitleAr = "الحسابات والتحويلات البنكية",
            Icon = "🏦",
            Methods = methods.Where(m => m.Category.Equals("banks", StringComparison.OrdinalIgnoreCase) || m.Category.Equals("bank", StringComparison.OrdinalIgnoreCase)).ToList()
        };

        var cardCategory = new PaymentCategory
        {
            Key = "cards",
            Title = "Debit & Credit Cards",
            TitleAr = "البطاقات الإلكترونية (فيزا / ماستر كارد)",
            Icon = "💳",
            Methods = methods.Where(m => m.Category.Equals("cards", StringComparison.OrdinalIgnoreCase) || m.Category.Equals("card", StringComparison.OrdinalIgnoreCase)).ToList()
        };

        var result = new List<PaymentCategory>();
        if (walletCategory.Methods.Count > 0) result.Add(walletCategory);
        if (bankCategory.Methods.Count > 0) result.Add(bankCategory);
        if (cardCategory.Methods.Count > 0) result.Add(cardCategory);

        return result;
    }

    private static List<PaymentMethodItem> GetFallbackMethods()
    {
        return new List<PaymentMethodItem>
        {
            new() { Code = "KUR", Name = "Kuraimi (M-Moyassar)", NameAr = "الكريمي (خدمة مميز)", Category = "banks", Icon = "🏦", Badge = "Direct Rail", BadgeAr = "مباشر" },
            new() { Code = "FLOOSAK", Name = "Floosak Wallet", NameAr = "محفظة فلوسك (بنك الكريمي)", Category = "wallets", Icon = "📱", Badge = "Instant", BadgeAr = "فوري", RequiresOtp = true },
            new() { Code = "JAWALI", Name = "Jawwali Wallet", NameAr = "محفظة جوالي (بنك اليمن والخليج)", Category = "wallets", Icon = "📲", Badge = "Instant", BadgeAr = "فوري" },
            new() { Code = "CASH", Name = "Cash Wallet", NameAr = "محفظة كاش", Category = "wallets", Icon = "💵", Badge = "Instant", BadgeAr = "فوري" },
            new() { Code = "SABACASH", Name = "SabaCash Wallet", NameAr = "محفظة سبأ كاش", Category = "wallets", Icon = "🪙", Badge = "Instant", BadgeAr = "فوري" },
            new() { Code = "ONECASH", Name = "OneCash Wallet", NameAr = "محفظة ون كاش", Category = "wallets", Icon = "💳", Badge = "Instant", BadgeAr = "فوري" },
            new() { Code = "WENET", Name = "WeNet", NameAr = "شبكة ون كاش (WeNet)", Category = "wallets", Icon = "🌐", Badge = "Network", BadgeAr = "شبكة" },
            new() { Code = "VISA", Name = "Visa / MasterCard", NameAr = "بطاقات فيزا وماستر كارد", Category = "cards", Icon = "💳", Badge = "Global", BadgeAr = "عالمي" }
        };
    }
}
