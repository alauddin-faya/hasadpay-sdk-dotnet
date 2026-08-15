namespace HasadPay.Net.Exceptions;

/// <summary>
/// Base exception class for all HasadPay SDK errors.
/// </summary>
public class HasadPayException : Exception
{
    public int? StatusCode { get; }
    public string? ErrorCode { get; }
    public string? RawResponse { get; }

    public HasadPayException(string message, int? statusCode = null, string? errorCode = null, string? rawResponse = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        RawResponse = rawResponse;
    }
}

/// <summary>
/// Thrown when authentication fails (invalid credentials, expired token, or unauthorized API key).
/// </summary>
public class HasadPayAuthenticationException : HasadPayException
{
    public HasadPayAuthenticationException(string message, int? statusCode = 401, string? errorCode = "AUTHENTICATION_FAILED", string? rawResponse = null)
        : base(message, statusCode, errorCode, rawResponse) { }
}

/// <summary>
/// Thrown when request payload fails validation (missing required fields, bad format, invalid amount).
/// </summary>
public class HasadPayValidationException : HasadPayException
{
    public Dictionary<string, string[]>? ValidationErrors { get; }

    public HasadPayValidationException(string message, Dictionary<string, string[]>? validationErrors = null, int? statusCode = 400, string? errorCode = "VALIDATION_ERROR", string? rawResponse = null)
        : base(message, statusCode, errorCode, rawResponse)
    {
        ValidationErrors = validationErrors;
    }
}

/// <summary>
/// Thrown when webhook HMAC-SHA256 signature verification fails.
/// </summary>
public class HasadPaySignatureException : HasadPayException
{
    public HasadPaySignatureException(string message = "Webhook HMAC-SHA256 signature verification failed.")
        : base(message, 400, "SIGNATURE_MISMATCH") { }
}

/// <summary>
/// Thrown when a requested transaction, invoice, or entity is not found.
/// </summary>
public class HasadPayNotFoundException : HasadPayException
{
    public HasadPayNotFoundException(string message, int? statusCode = 404, string? errorCode = "NOT_FOUND", string? rawResponse = null)
        : base(message, statusCode, errorCode, rawResponse) { }
}

/// <summary>
/// Thrown when the HasadPay API returns a generic non-2xx error.
/// </summary>
public class HasadPayApiException : HasadPayException
{
    public HasadPayApiException(string message, int statusCode, string? errorCode = null, string? rawResponse = null)
        : base(message, statusCode, errorCode, rawResponse) { }
}

/// <summary>
/// Thrown when a network failure, timeout, or DNS resolution error occurs.
/// </summary>
public class HasadPayNetworkException : HasadPayException
{
    public HasadPayNetworkException(string message, Exception? innerException = null)
        : base(message, null, "NETWORK_ERROR", null, innerException) { }
}
