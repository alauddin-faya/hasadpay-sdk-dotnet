using HasadPay.Net;
using HasadPay.Net.Exceptions;
using HasadPay.Net.Webhooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetCoreApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly HasadPayOptions _options;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IOptions<HasadPayOptions> options,
        ILogger<WebhookController> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Handles incoming HasadPay HMAC-SHA256 verified webhook events.
    /// </summary>
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

            _logger.LogInformation("HasadPay Webhook verified: Event={Event}, ID={Id}", webhookEvent.Event, webhookEvent.Id);

            if (webhookEvent.IsPaymentSucceeded())
            {
                _logger.LogInformation("✅ Payment captured for Transaction ID: {TxId}, Amount: {Amount} {Currency}",
                    webhookEvent.Data.Id ?? webhookEvent.Id,
                    webhookEvent.Data.Amount,
                    webhookEvent.Data.Currency);

                // TODO: Mark order as Paid in your database
            }
            else if (webhookEvent.IsPaymentFailed())
            {
                _logger.LogWarning("❌ Payment failed for Transaction ID: {TxId}", webhookEvent.Data.Id ?? webhookEvent.Id);

                // TODO: Mark order as Failed in your database
            }

            return Ok(new { status = "received", id = webhookEvent.Id });
        }
        catch (HasadPaySignatureException ex)
        {
            _logger.LogWarning("Webhook signature mismatch: {Message}", ex.Message);
            return BadRequest(new { status = "error", message = "Invalid signature" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process webhook payload.");
            return BadRequest(new { status = "error", message = "Malformed payload" });
        }
    }
}
