namespace Smart.ProxyPilot.Models;

/// <summary>
/// 代理池内部状态对象（可变）。
/// </summary>
internal sealed class ProxyPoolState() : IProxyPoolState
{
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    public int TotalCount { get; private set; }
    public int PendingCount { get; private set; }
    public int ValidatingCount { get; private set; }
    public int AvailableCount { get; private set; }
    public int InUseCount { get; private set; }
    public int CooldownCount { get; private set; }
    public int DisabledCount { get; private set; }

    public long TotalValidations { get; private set; }
    public long SuccessfulValidations { get; private set; }
    public long FailedValidations { get; private set; }

    public long TotalGetRequests { get; private set; }
    public long SuccessfulGetRequests { get; private set; }
    public long WaitingGetRequests { get; private set; }

    public double AvgValidationTime { get; private set; }
    public double AvgResponseTime { get; private set; }
    public double OverallSuccessRate { get; private set; }

    private double _totalValidationMs;
    private double _totalResponseMs;

    /// <summary>
    /// 新增代理时更新状态。
    /// </summary>
    public void AddProxy(ProxyState state)
    {
        switch (state)
        {
            case ProxyState.Pending:
                PendingCount++;
                break;
            case ProxyState.Validating:
                ValidatingCount++;
                break;
            case ProxyState.Available:
                AvailableCount++;
                break;
            case ProxyState.InUse:
                InUseCount++;
                break;
            case ProxyState.Cooldown:
                CooldownCount++;
                break;
            case ProxyState.Disabled:
                DisabledCount++;
                break;
        }

        TotalCount++;
        Touch();
    }

    /// <summary>
    /// 移除代理时更新状态。
    /// </summary>
    public void RemoveProxy(ProxyState state)
    {
        switch (state)
        {
            case ProxyState.Pending:
                PendingCount = Math.Max(0, PendingCount - 1);
                break;
            case ProxyState.Validating:
                ValidatingCount = Math.Max(0, ValidatingCount - 1);
                break;
            case ProxyState.Available:
                AvailableCount = Math.Max(0, AvailableCount - 1);
                break;
            case ProxyState.InUse:
                InUseCount = Math.Max(0, InUseCount - 1);
                break;
            case ProxyState.Cooldown:
                CooldownCount = Math.Max(0, CooldownCount - 1);
                break;
            case ProxyState.Disabled:
                DisabledCount = Math.Max(0, DisabledCount - 1);
                break;
        }

        TotalCount = Math.Max(0, TotalCount - 1);
        Touch();
    }

    /// <summary>
    /// 状态流转时更新计数。
    /// </summary>
    public void ChangeState(ProxyState oldState, ProxyState newState)
    {
        DecrementStateCount(oldState);
        IncrementStateCount(newState);
        Touch();
    }

    private void IncrementStateCount(ProxyState state)
    {
        switch (state)
        {
            case ProxyState.Pending:
                PendingCount++;
                break;
            case ProxyState.Validating:
                ValidatingCount++;
                break;
            case ProxyState.Available:
                AvailableCount++;
                break;
            case ProxyState.InUse:
                InUseCount++;
                break;
            case ProxyState.Cooldown:
                CooldownCount++;
                break;
            case ProxyState.Disabled:
                DisabledCount++;
                break;
        }
    }

    private void DecrementStateCount(ProxyState state)
    {
        switch (state)
        {
            case ProxyState.Pending:
                PendingCount = Math.Max(0, PendingCount - 1);
                break;
            case ProxyState.Validating:
                ValidatingCount = Math.Max(0, ValidatingCount - 1);
                break;
            case ProxyState.Available:
                AvailableCount = Math.Max(0, AvailableCount - 1);
                break;
            case ProxyState.InUse:
                InUseCount = Math.Max(0, InUseCount - 1);
                break;
            case ProxyState.Cooldown:
                CooldownCount = Math.Max(0, CooldownCount - 1);
                break;
            case ProxyState.Disabled:
                DisabledCount = Math.Max(0, DisabledCount - 1);
                break;
        }
    }

    /// <summary>
    /// 记录验证统计。
    /// </summary>
    public void RecordValidation(ValidationResult result)
    {
        TotalValidations++;
        if (result.IsSuccess)
        {
            SuccessfulValidations++;
        }
        else
        {
            FailedValidations++;
        }

        _totalValidationMs += result.ResponseTime.TotalMilliseconds;
        _totalResponseMs += result.ResponseTime.TotalMilliseconds;
        AvgValidationTime = TotalValidations > 0 ? _totalValidationMs / TotalValidations : 0;
        AvgResponseTime = TotalValidations > 0 ? _totalResponseMs / TotalValidations : 0;
        OverallSuccessRate = TotalValidations > 0 ? (double)SuccessfulValidations / TotalValidations : 0;
        Touch();
    }

    /// <summary>
    /// 记录获取请求。
    /// </summary>
    public void RecordGetRequest()
    {
        TotalGetRequests++;
        Touch();
    }

    /// <summary>
    /// 记录获取成功。
    /// </summary>
    public void RecordGetSuccess()
    {
        SuccessfulGetRequests++;
        Touch();
    }

    /// <summary>
    /// 等待获取计数 +1。
    /// </summary>
    public void IncrementWaiting()
    {
        WaitingGetRequests++;
        Touch();
    }

    /// <summary>
    /// 等待获取计数 -1。
    /// </summary>
    public void DecrementWaiting()
    {
        WaitingGetRequests = Math.Max(0, WaitingGetRequests - 1);
        Touch();
    }

    private void Touch() => Timestamp = DateTime.UtcNow;
}
