using HasadPay.Net.Models.Common;
using HasadPay.Net.Models.Transactions;
using HasadPay.Net.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly ITransactionsService _transactions;
    private readonly IPaymentMethodsService _methods;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(
        ITransactionsService transactions,
        IPaymentMethodsService methods,
        ILogger<CheckoutController> logger)
    {
        _transactions = transactions;
        _methods = methods;
        _logger = logger;
    }

    /// <summary>
    /// Discovers active payment rails and methods.
    /// </summary>
    [HttpGet("methods")]
    public async Task<IActionResult> GetMethods()
    {
        var categorized = await _methods.GetCategorizedMethodsAsync();
        return Ok(categorized);
    }

    /// <summary>
    /// Creates a payment transaction session and returns checkout URL.
    /// </summary>
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiateCheckout([FromBody] InitiateCheckoutDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerMobile))
        {
            return BadRequest(new { error = "رقم الهاتف المحمول مطلوب للمتابعة." });
        }

        try
        {
            string orderNumber = $"ORD-NET-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            string callbackUrl = $"{Request.Scheme}://{Request.Host}/payment/callback?order_number={orderNumber}";

            var request = new TransactionCreateRequest
            {
                Amount = dto.Amount.ToString("F2"),
                Currency = dto.Currency ?? "YER",
                MobileNumber = dto.CustomerMobile,
                RequestReference = orderNumber,
                Name = dto.CustomerName ?? "عميل متجر الدوت نت",
                Address = dto.Address ?? "صنعاء، اليمن",
                ReturnUrl = callbackUrl,
                Service = string.IsNullOrWhiteSpace(dto.Service) ? null : dto.Service,
                Items = dto.Items ?? new List<OrderItem>
                {
                    new("منتج تجريبي", dto.Amount, 1)
                }
            };

            var tx = await _transactions.CreateAsync(request);

            return Ok(new
            {
                status = "ok",
                checkout_url = tx.CheckoutUrl,
                checkout_id = tx.Uuid,
                order_number = orderNumber
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate HasadPay checkout.");
            return BadRequest(new { status = "error", error = ex.Message });
        }
    }

    /// <summary>
    /// Inquires about transaction status from callback.
    /// </summary>
    [HttpGet("status/{id}")]
    public async Task<IActionResult> GetStatus(string id)
    {
        try
        {
            var status = await _transactions.GetAsync(id);
            return Ok(status);
        }
        catch (Exception ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

public class InitiateCheckoutDto
{
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? CustomerName { get; set; }
    public string CustomerMobile { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Service { get; set; }
    public List<OrderItem>? Items { get; set; }
}
