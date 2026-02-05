namespace Smart.ProxyPilot.Models;

public class ProxyStatistics()
{
    public int TotalValidationCount { get; private set; }
    public int ValidationSuccessCount { get; private set; }
    public int ValidationFailCount { get; private set; }
    public int ValidationTimeoutCount { get; private set; }

    public int TotalUseCount { get; private set; }
    public int UseSuccessCount { get; private set; }
    public int UseFailCount { get; private set; }
    public int UseTimeoutCount { get; private set; }

    public double MinResponseTime { get; private set; }
    public double MaxResponseTime { get; private set; }
    public double AvgResponseTime { get; private set; }
    public double LastResponseTime { get; private set; }

    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime? FirstValidatedAt { get; private set; }
    public DateTime? LastValidatedAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public DateTime? LastSuccessAt { get; private set; }
    public DateTime? LastFailAt { get; private set; }

    public int ConsecutiveFailCount { get; private set; }
    public int ConsecutiveSuccessCount { get; private set; }

    public double ValidationSuccessRate => TotalValidationCount > 0
        ? (double)ValidationSuccessCount / TotalValidationCount
        : 0;

    public double UseSuccessRate => TotalUseCount > 0
        ? (double)UseSuccessCount / TotalUseCount
        : 0;

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
