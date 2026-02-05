namespace Smart.ProxyPilot.Models;

public class ValidationResult
{
    private ValidationResult()
    {
    }

    public bool IsSuccess { get; private set; }
    public ValidationResultType ResultType { get; private set; }
    public TimeSpan ResponseTime { get; private set; }
    public int? StatusCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Exception? Exception { get; private set; }
    public DateTime ValidatedAt { get; private set; }

    public static ValidationResult Success(TimeSpan responseTime, int statusCode)
        => new()
        {
            IsSuccess = true,
            ResultType = ValidationResultType.Success,
            ResponseTime = responseTime,
            StatusCode = statusCode,
            ValidatedAt = DateTime.UtcNow
        };

    public static ValidationResult Timeout(TimeSpan elapsed)
        => new()
        {
            IsSuccess = false,
            ResultType = ValidationResultType.Timeout,
            ResponseTime = elapsed,
            ErrorMessage = "Timeout",
            ValidatedAt = DateTime.UtcNow
        };

    public static ValidationResult Failed(ValidationResultType type, string message, Exception? ex = null)
        => new()
        {
            IsSuccess = false,
            ResultType = type,
            ResponseTime = TimeSpan.Zero,
            ErrorMessage = message,
            Exception = ex,
            ValidatedAt = DateTime.UtcNow
        };
}
