namespace Smart.ProxyPilot.Models;

public class ValidationResult
{
    private ValidationResult()
    {
    }

    /// <summary>
    /// 是否成功。
    /// </summary>
    public bool IsSuccess { get; private set; }
    /// <summary>
    /// 结果类型。
    /// </summary>
    public ValidationResultType ResultType { get; private set; }
    /// <summary>
    /// 响应时间。
    /// </summary>
    public TimeSpan ResponseTime { get; private set; }
    /// <summary>
    /// HTTP 状态码。
    /// </summary>
    public int? StatusCode { get; private set; }
    /// <summary>
    /// 错误信息。
    /// </summary>
    public string? ErrorMessage { get; private set; }
    /// <summary>
    /// 异常信息。
    /// </summary>
    public Exception? Exception { get; private set; }
    /// <summary>
    /// 验证时间。
    /// </summary>
    public DateTime ValidatedAt { get; private set; }

    /// <summary>
    /// 成功结果。
    /// </summary>
    /// <param name="responseTime">响应时间。</param>
    /// <param name="statusCode">状态码。</param>
    public static ValidationResult Success(TimeSpan responseTime, int statusCode)
        => new()
        {
            IsSuccess = true,
            ResultType = ValidationResultType.Success,
            ResponseTime = responseTime,
            StatusCode = statusCode,
            ValidatedAt = DateTime.UtcNow
        };

    /// <summary>
    /// 超时结果。
    /// </summary>
    /// <param name="elapsed">耗时。</param>
    public static ValidationResult Timeout(TimeSpan elapsed)
        => new()
        {
            IsSuccess = false,
            ResultType = ValidationResultType.Timeout,
            ResponseTime = elapsed,
            ErrorMessage = "Timeout",
            ValidatedAt = DateTime.UtcNow
        };

    /// <summary>
    /// 失败结果。
    /// </summary>
    /// <param name="type">失败类型。</param>
    /// <param name="message">错误信息。</param>
    /// <param name="ex">异常。</param>
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
