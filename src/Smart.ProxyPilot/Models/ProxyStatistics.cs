namespace Smart.ProxyPilot.Models;

public class ProxyStatistics()
{
    /// <summary>
    /// 验证总次数。
    /// </summary>
    public int TotalValidationCount { get; private set; }
    /// <summary>
    /// 验证成功次数。
    /// </summary>
    public int ValidationSuccessCount { get; private set; }
    /// <summary>
    /// 验证失败次数。
    /// </summary>
    public int ValidationFailCount { get; private set; }
    /// <summary>
    /// 验证超时次数。
    /// </summary>
    public int ValidationTimeoutCount { get; private set; }

    /// <summary>
    /// 使用总次数。
    /// </summary>
    public int TotalUseCount { get; private set; }
    /// <summary>
    /// 使用成功次数。
    /// </summary>
    public int UseSuccessCount { get; private set; }
    /// <summary>
    /// 使用失败次数。
    /// </summary>
    public int UseFailCount { get; private set; }
    /// <summary>
    /// 使用超时次数。
    /// </summary>
    public int UseTimeoutCount { get; private set; }

    /// <summary>
    /// 最小响应时间（毫秒）。
    /// </summary>
    public double MinResponseTime { get; private set; }
    /// <summary>
    /// 最大响应时间（毫秒）。
    /// </summary>
    public double MaxResponseTime { get; private set; }
    /// <summary>
    /// 平均响应时间（毫秒）。
    /// </summary>
    public double AvgResponseTime { get; private set; }
    /// <summary>
    /// 最近一次响应时间（毫秒）。
    /// </summary>
    public double LastResponseTime { get; private set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    /// <summary>
    /// 首次验证时间。
    /// </summary>
    public DateTime? FirstValidatedAt { get; private set; }
    /// <summary>
    /// 最近验证时间。
    /// </summary>
    public DateTime? LastValidatedAt { get; private set; }
    /// <summary>
    /// 最近使用时间。
    /// </summary>
    public DateTime? LastUsedAt { get; private set; }
    /// <summary>
    /// 最近成功时间。
    /// </summary>
    public DateTime? LastSuccessAt { get; private set; }
    /// <summary>
    /// 最近失败时间。
    /// </summary>
    public DateTime? LastFailAt { get; private set; }

    /// <summary>
    /// 连续失败次数。
    /// </summary>
    public int ConsecutiveFailCount { get; private set; }
    /// <summary>
    /// 连续成功次数。
    /// </summary>
    public int ConsecutiveSuccessCount { get; private set; }

    /// <summary>
    /// 验证成功率。
    /// </summary>
    public double ValidationSuccessRate => TotalValidationCount > 0
        ? (double)ValidationSuccessCount / TotalValidationCount
        : 0;

    /// <summary>
    /// 使用成功率。
    /// </summary>
    public double UseSuccessRate => TotalUseCount > 0
        ? (double)UseSuccessCount / TotalUseCount
        : 0;

    /// <summary>
    /// 记录验证结果。
    /// </summary>
    /// <param name="result">验证结果。</param>
    public void RecordValidation(ValidationResult result)
    {
        TotalValidationCount++;
        LastValidatedAt = result.ValidatedAt;
        FirstValidatedAt ??= result.ValidatedAt;

        if (result.IsSuccess)
        {
            ValidationSuccessCount++;
            ConsecutiveSuccessCount++;
            ConsecutiveFailCount = 0;
            LastSuccessAt = result.ValidatedAt;
        }
        else
        {
            ValidationFailCount++;
            ConsecutiveFailCount++;
            ConsecutiveSuccessCount = 0;
            LastFailAt = result.ValidatedAt;
            if (result.ResultType == ValidationResultType.Timeout)
            {
                ValidationTimeoutCount++;
            }
        }

        UpdateResponseTime(result.ResponseTime);
    }

    /// <summary>
    /// 记录使用结果。
    /// </summary>
    /// <param name="success">是否成功。</param>
    /// <param name="responseTime">响应时间。</param>
    /// <param name="resultType">结果类型。</param>
    public void RecordUse(bool success, TimeSpan? responseTime, ValidationResultType resultType)
    {
        TotalUseCount++;
        LastUsedAt = DateTime.UtcNow;

        if (success)
        {
            UseSuccessCount++;
            ConsecutiveSuccessCount++;
            ConsecutiveFailCount = 0;
            LastSuccessAt = DateTime.UtcNow;
        }
        else
        {
            UseFailCount++;
            ConsecutiveFailCount++;
            ConsecutiveSuccessCount = 0;
            LastFailAt = DateTime.UtcNow;
            if (resultType == ValidationResultType.Timeout)
            {
                UseTimeoutCount++;
            }
        }

        if (responseTime.HasValue)
        {
            UpdateResponseTime(responseTime.Value);
        }
    }

    private void UpdateResponseTime(TimeSpan responseTime)
    {
        var ms = responseTime.TotalMilliseconds;
        LastResponseTime = ms;

        if (TotalValidationCount + TotalUseCount == 1)
        {
            MinResponseTime = ms;
            MaxResponseTime = ms;
            AvgResponseTime = ms;
            return;
        }

        MinResponseTime = MinResponseTime == 0 ? ms : Math.Min(MinResponseTime, ms);
        MaxResponseTime = Math.Max(MaxResponseTime, ms);
        var totalCount = TotalValidationCount + TotalUseCount;
        AvgResponseTime = ((AvgResponseTime * (totalCount - 1)) + ms) / totalCount;
    }
}
