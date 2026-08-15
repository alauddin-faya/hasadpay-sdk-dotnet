using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HasadPay.Net.Exceptions;
using HasadPay.Net.Models.Webhooks;

namespace HasadPay.Net.Webhooks;

/// <summary>
/// Cryptographic security helper for verifying and constructing HasadPay Webhook events.
/// </summary>
public static class HasadPayWebhook
{
    /// <summary>
    /// Verifies the HMAC-SHA256 signature and constructs a strongly-typed <see cref="WebhookEvent"/>.
    /// </summary>
    /// <param name="payload">Raw HTTP request body string.</param>
    /// <param name="signature">The signature received from 'X-HasadPay-Signature' or 'X-Signature' header.</param>
    /// <param name="secret">Your merchant webhook secret key.</param>
    /// <param name="toleranceSeconds">Optional timestamp tolerance to prevent replay attacks (Default: 300s / 5min). Pass 0 to disable.</param>
    /// <returns>Strongly-typed <see cref="WebhookEvent"/>.</returns>
    /// <exception cref="HasadPaySignatureException">Thrown when signature verification fails.</exception>
    /// <exception cref="HasadPayValidationException">Thrown when payload is malformed.</exception>
    public static WebhookEvent ConstructEvent(
        string payload,
        string? signature,
        string? secret,
        long toleranceSeconds = 300)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new HasadPayValidationException("Webhook payload cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException("Webhook secret must be provided.", nameof(secret));
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new HasadPaySignatureException("Missing X-HasadPay-Signature or X-Signature header.");
        }

        bool isValid = VerifySignature(payload, signature, secret);
        if (!isValid)
        {
            throw new HasadPaySignatureException("Webhook HMAC-SHA256 signature verification failed. Signature mismatch.");
        }

        try
        {
            var webhookEvent = JsonSerializer.Deserialize<WebhookEvent>(payload, HasadPayClient.JsonOptions);
            if (webhookEvent == null)
            {
                throw new HasadPayValidationException("Failed to deserialize webhook event JSON.");
            }

            if (toleranceSeconds > 0 && webhookEvent.Timestamp > 0)
            {
                long currentUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (Math.Abs(currentUnix - webhookEvent.Timestamp) > toleranceSeconds)
                {
                    throw new HasadPaySignatureException($"Webhook timestamp is outside tolerance window ({toleranceSeconds}s).");
                }
            }

            return webhookEvent;
        }
        catch (JsonException ex)
        {
            throw new HasadPayValidationException($"Malformed webhook JSON payload: {ex.Message}", innerException: ex);
        }
    }

    /// <summary>
    /// Verifies the HMAC-SHA256 signature using constant-time comparison to prevent timing attacks.
    /// </summary>
    public static bool VerifySignature(string payload, string signature, string secret)
    {
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        string cleanSignature = signature.Trim();
        if (cleanSignature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
        {
            cleanSignature = cleanSignature.Substring(7);
        }

        byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(secretBytes);
        byte[] hash = hmac.ComputeHash(payloadBytes);
        string expectedHex = Convert.ToHexString(hash).ToLowerInvariant();

        byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedHex);
        byte[] actualBytes = Encoding.UTF8.GetBytes(cleanSignature.ToLowerInvariant());

        if (expectedBytes.Length != actualBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
