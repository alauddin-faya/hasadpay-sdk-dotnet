using HasadPay.Net;
using HasadPay.Net.Enums;
using HasadPay.Net.Models.Common;
using HasadPay.Net.Models.Invoices;
using HasadPay.Net.Models.Transactions;
using HasadPay.Net.Webhooks;

Console.WriteLine("==================================================");
Console.WriteLine("   HasadPay .NET Core / C# SDK Demonstration      ");
Console.WriteLine("==================================================");

// 1. Initialize Client Options
var options = new HasadPayOptions
{
    BaseUrl = "https://merchent-local.fintechsys.net",
    Username = "alafaya",
    Password = "alauddin@123",
    EntityId = "046a8e21-6e56-440d-8f32-a078c63dfc64",
    WebhookSecret = "whsec_demo_secret_2026",
    DefaultCurrency = "YER"
};

using var client = new HasadPayClient(options);

Console.WriteLine("\n[1] Discovering Available Payment Rails...");
try
{
    var categories = await client.Methods.GetCategorizedMethodsAsync();
    foreach (var cat in categories)
    {
        Console.WriteLine($"\n📁 {cat.Icon} {cat.TitleAr} ({cat.Title}):");
        foreach (var method in cat.Methods)
        {
            Console.WriteLine($"   • [{method.Code}] {method.NameAr} - {method.BadgeAr}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   ⚠️ Discovery Note: {ex.Message}");
}

// 2. Way 1: Create Standard Hosted Checkout Transaction (No Service Code)
Console.WriteLine("\n[2] Creating Hosted Multi-Rail Transaction (Way 1)...");
try
{
    var hostedTx = await client.Transactions.CreateAsync(new TransactionCreateRequest
    {
        Amount = "1500.00",
        Currency = Currency.YER.ToCode(),
        MobileNumber = "771234567",
        RequestReference = $"ORDER-NET-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
        Name = "علاءالدين فايع",
        Address = "اليمن، صنعاء",
        ReturnUrl = "https://myshop.com/payment/callback",
        Items = new List<OrderItem>
        {
            new("سماعات بلوتوث برو", 1500.00m, 1, sku: "HP-01")
        }
    });

    Console.WriteLine($"   ✅ Transaction Created Successfully!");
    Console.WriteLine($"      • UUID: {hostedTx.Uuid}");
    Console.WriteLine($"      • Status: {hostedTx.Status} ({hostedTx.StatusDisplay})");
    Console.WriteLine($"      • Checkout URL: {hostedTx.CheckoutUrl}");
}
catch (Exception ex)
{
    Console.WriteLine($"   ❌ Error: {ex.Message}");
}

// 3. Way 2: Direct Rail Payment (e.g. Floosak with OTP confirmation)
Console.WriteLine("\n[3] Creating Direct Rail Payment (Floosak - Way 2)...");
try
{
    var directTx = await client.Transactions.CreateAsync(new TransactionCreateRequest
    {
        Amount = "500.00",
        Currency = Currency.YER.ToCode(),
        MobileNumber = "771234567",
        RequestReference = $"ORDER-DIR-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
        Name = "أحمد محمد",
        Service = PaymentBrand.Floosak,
        ReturnUrl = "https://myshop.com/payment/callback"
    });

    Console.WriteLine($"   ✅ Direct Rail Initiated!");
    Console.WriteLine($"      • UUID: {directTx.Uuid}");
    Console.WriteLine($"      • Pending OTP: {directTx.IsPending()}");
}
catch (Exception ex)
{
    Console.WriteLine($"   ❌ Error: {ex.Message}");
}

// 4. Create an Invoice
Console.WriteLine("\n[4] Creating an Invoice...");
try
{
    var invoice = await client.Invoices.CreateAsync(new InvoiceCreateRequest
    {
        Amount = "3500.00",
        Currency = "YER",
        CustomerName = "سارة أحمد",
        CustomerMobile = "777889900",
        MerchantReference = $"INV-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
        Title = "فاتورة دورة تدريبية",
        Description = "رسوم الاشتراك في الدورة البرمجية المتقدمة",
        AllowPartialPayment = true
    });

    Console.WriteLine($"   ✅ Invoice Created!");
    Console.WriteLine($"      • Number: {invoice.InvoiceNumber}");
    Console.WriteLine($"      • URL: {invoice.CheckoutUrl}");
}
catch (Exception ex)
{
    Console.WriteLine($"   ❌ Error: {ex.Message}");
}

// 5. Test Webhook Signature Verification
Console.WriteLine("\n[5] Testing Webhook HMAC-SHA256 Verification...");
string samplePayload = """{"event":"payment.succeeded","id":"tx_abc123","timestamp":1700000000,"data":{"status":"success","amount":"1500.00","currency":"YER"}}""";
using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(options.WebhookSecret!)))
{
    string sampleSig = Convert.ToHexString(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(samplePayload))).ToLowerInvariant();
    try
    {
        var evt = HasadPayWebhook.ConstructEvent(samplePayload, sampleSig, options.WebhookSecret, toleranceSeconds: 0);
        Console.WriteLine($"   ✅ Webhook Verified: Event={evt.Event}, ID={evt.Id}, Succeeded={evt.IsPaymentSucceeded()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ❌ Signature Error: {ex.Message}");
    }
}

Console.WriteLine("\nDemo Complete! Press any key to exit.");
