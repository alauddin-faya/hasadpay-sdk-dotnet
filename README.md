# HasadPay .NET Core / C# SDK (`HasadPay.Net`)

[![NuGet Version](https://img.shields.io/badge/nuget-v1.0.0-blue.svg)](https://www.nuget.org)
[![.NET Compatibility](https://img.shields.io/badge/.NET-8.0%20%7C%20Standard%202.1-purple.svg)](https://dotnet.microsoft.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Official .NET Core and C# SDK for the **HasadPay** Unified Payment Gateway.

Integrate seamless, multi-wallet electronic payments in Yemen and the MENA region with support for **Kuraimi (M-Moyassar)**, **Floosak**, **Jawwali**, **Cash**, **SabaCash**, **OneCash**, **WeNet**, and **Visa/MasterCard**.

---

## 🌟 Key Features

- **Modern C# & .NET 8.0 / .NET Standard 2.1**: Async/await with `CancellationToken` throughout.
- **Dual Authentication Modes**:
  - **Auto JWT Authentication**: Automatic login, thread-safe token caching, and transparent retry on token expiration (401).
  - **Static API Key & Bearer Token**: Flexible authorization header management.
- **Way 1: Hosted Multi-Rail Checkout**: Creates a session omitting `Service` to allow the customer to choose their preferred wallet on HasadPay's hosted payment screen.
- **Way 2: Direct Rail Payment**: Passes a specific service code (e.g. `PaymentBrand.Floosak`) with OTP/PIN confirmation workflows.
- **Invoicing Engine**: Generate, query, and cancel invoices with split beneficiaries.
- **Webhook Security**: Constant-time HMAC-SHA256 signature verification to prevent timing and replay attacks.
- **First-Class ASP.NET Core Integration**: Native `IServiceCollection.AddHasadPay()` Dependency Injection with `IHttpClientFactory`.

---

## 📦 Installation

Install the package via NuGet CLI or Package Manager:

```bash
dotnet add package HasadPay.Net
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package HasadPay.Net
```

---

## 🚀 Quick Start (ASP.NET Core)

### 1. Configure `appsettings.json`

```json
{
  "HasadPay": {
    "BaseUrl": "https://merchent-local.fintechsys.net",
    "Username": "your_merchant_username",
    "Password": "your_merchant_password",
    "EntityId": "046a8e21-6e56-440d-8f32-a078c63dfc64",
    "WebhookSecret": "whsec_your_webhook_secret_key",
    "DefaultCurrency": "YER",
    "TimeoutSeconds": 30
  }
}
```

### 2. Register in `Program.cs`

```csharp
using HasadPay.Net.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register HasadPay Client and Services
builder.Services.AddHasadPay(builder.Configuration);

builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();
app.Run();
```

---

## 💳 Usage Examples

### Way 1: Hosted Multi-Rail Checkout (Recommended)

In this flow, you omit the `Service` parameter. The customer is redirected to HasadPay's hosted checkout screen to choose their wallet or card:

```csharp
using HasadPay.Net.Enums;
using HasadPay.Net.Models.Common;
using HasadPay.Net.Models.Transactions;
using HasadPay.Net.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ITransactionsService _transactions;

    public PaymentController(ITransactionsService transactions)
    {
        _transactions = transactions;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckout()
    {
        var response = await _transactions.CreateAsync(new TransactionCreateRequest
        {
            Amount = "1500.00",
            Currency = Currency.YER.ToCode(),
            MobileNumber = "771234567",
            RequestReference = $"ORDER-{Guid.NewGuid():N}",
            Name = "علاءالدين فايع",
            Address = "صنعاء، شارع حدة",
            ReturnUrl = "https://myshop.com/payment/callback",
            Items = new List<OrderItem>
            {
                new("سماعات بلوتوث برو لاسلكية", 1500.00m, 1, sku: "HP-01")
            }
        });

        // Redirect customer to the secure hosted checkout page
        return Ok(new
        {
            checkout_url = response.CheckoutUrl,
            transaction_id = response.Uuid
        });
    }
}
```

---

### Way 2: Direct Rail Payment (e.g. Floosak with OTP Confirmation)

```csharp
// 1. Initiate direct wallet payment
var transaction = await client.Transactions.CreateAsync(new TransactionCreateRequest
{
    Amount = "500.00",
    Currency = "YER",
    MobileNumber = "771234567",
    RequestReference = "ORDER-DIR-1001",
    Name = "محمد العلي",
    Service = PaymentBrand.Floosak,
    ReturnUrl = "https://myshop.com/payment/callback"
});

// 2. If OTP is required by the wallet, confirm with OTP
if (transaction.IsPending())
{
    var confirmed = await client.Transactions.ConfirmAsync(
        transactionId: transaction.Uuid,
        otp: "123456"
    );

    if (confirmed.IsSuccessful())
    {
        Console.WriteLine("Payment Completed Successfully!");
    }
}
```

---

### 🔍 Querying Real-Time Transaction Status

```csharp
var status = await client.Transactions.GetAsync("1f9c82fc-0de7-4810-b530-27df4d6f8f11");

if (status.IsSuccessful())
{
    Console.WriteLine($"Payment Captured: {status.Amount} {status.Currency}");
}
else if (status.IsPending())
{
    Console.WriteLine("Transaction is still awaiting customer action.");
}
else
{
    Console.WriteLine($"Transaction Failed: {status.StatusDisplay}");
}
```

---

### 📄 Invoices Engine

```csharp
// Create a new invoice
var invoice = await client.Invoices.CreateAsync(new InvoiceCreateRequest
{
    Amount = "3500.00",
    Currency = "YER",
    CustomerName = "سارة أحمد",
    CustomerMobile = "777889900",
    MerchantReference = "INV-2026-0091",
    Title = "فاتورة اشتراك سنوي",
    Description = "رسوم تجديد الاشتراك في المنصة البرمجية",
    AllowPartialPayment = true
});

Console.WriteLine($"Invoice Link: {invoice.CheckoutUrl}");

// Retrieve status
var invoiceDetails = await client.Invoices.GetAsync(invoice.InvoiceNumber);
if (invoiceDetails.IsPaid())
{
    Console.WriteLine("Invoice is fully paid!");
}
```

---

### 🛡️ Webhook Signature Verification

Secure your webhook endpoint against tampering and replay attacks using constant-time HMAC-SHA256 verification:

```csharp
using HasadPay.Net;
using HasadPay.Net.Exceptions;
using HasadPay.Net.Webhooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly HasadPayOptions _options;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IOptions<HasadPayOptions> options, ILogger<WebhookController> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        string signature = Request.Headers["X-HasadPay-Signature"].FirstOrDefault()
            ?? Request.Headers["X-Signature"].FirstOrDefault()
            ?? string.Empty;

        using var reader = new StreamReader(Request.Body);
        string payload = await reader.ReadToEndAsync();

        try
        {
            var webhookEvent = HasadPayWebhook.ConstructEvent(
                payload: payload,
                signature: signature,
                secret: _options.WebhookSecret
            );

            _logger.LogInformation("Webhook Verified: Event={Event}, ID={Id}", webhookEvent.Event, webhookEvent.Id);

            if (webhookEvent.IsPaymentSucceeded())
            {
                // Update Order state in DB to Paid
                string? txId = webhookEvent.Data.Id ?? webhookEvent.Id;
                _logger.LogInformation("Order paid for transaction: {TxId}", txId);
            }

            return Ok(new { status = "received" });
        }
        catch (HasadPaySignatureException ex)
        {
            _logger.LogWarning("Signature mismatch: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid signature" });
        }
    }
}
```

---

## 🏗️ Standalone Client (Console / Background Services)

```csharp
using HasadPay.Net;

var options = new HasadPayOptions
{
    BaseUrl = "https://merchent-local.fintechsys.net",
    Username = "your_username",
    Password = "your_password",
    EntityId = "your_entity_id"
};

using var client = new HasadPayClient(options);
var tx = await client.Transactions.CreateAsync(new() { ... });
```

---

## 🌐 دليل المطورين باللغة العربية (Arabic Guide)

حزمة **HasadPay.Net** هي الحزمة الرسمية لمنصة .NET Core و C# لربط بوابة الدفع الإلكتروني الموحدة **حصاد باي (HasadPay)**.

### التثبيت السريع:
```bash
dotnet add package HasadPay.Net
```

### المميزات الأساسية:
1. **دعم كامل لجميع المحافظ والبنوك اليمنية**: الكريمي مميز، محفظة فلوسك، جوالي، كاش، سبأ كاش، ون كاش، شبكة WeNet، وبطاقات فيزا/ماستر كارد.
2. **تسجيل الدخول التلقائي (Auto JWT)**: إدارة الرموز المميزة وتجديدها تلقائياً عند انتهاء صلاحيتها دون تدخل يدوي.
3. **طريقتين للدفع**:
   - **صفحة الدفع الموحدة (Way 1 - Hosted Checkout)**: توجيه العميل لشاشة السداد لاختيار وسيلته المفضلة.
   - **الدفع المباشر عبر رمز الخدمة (Way 2 - Direct Service)**: اختيار وسيلة دفع محددة مع دعم تأكيد كود OTP.
4. **حماية التواقيع الإلكترونية (HMAC-SHA256)**: التحقق الآمن من إشعارات الويب هوك لحماية بيانات المبيعات.

---

## 📄 License

This SDK is open-sourced software licensed under the [MIT license](LICENSE).
