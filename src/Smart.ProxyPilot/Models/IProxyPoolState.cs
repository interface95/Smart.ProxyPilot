namespace Smart.ProxyPilot.Models;

public interface IProxyPoolState
{
    /// <summary>
    /// 状态更新时间。
    /// </summary>
    DateTime Timestamp { get; }

    /// <summary>
    /// 总数量。
    /// </summary>
    int TotalCount { get; }
    /// <summary>
    /// 待验证数量。
    /// </summary>
    int PendingCount { get; }
    /// <summary>
    /// 验证中数量。
    /// </summary>
    int ValidatingCount { get; }
    /// <summary>
    /// 可用数量。
    /// </summary>
    int AvailableCount { get; }
    /// <summary>
    /// 使用中数量。
    /// </summary>
    int InUseCount { get; }
    /// <summary>
    /// 冷却中数量。
    /// </summary>
    int CooldownCount { get; }
    /// <summary>
    /// 禁用数量。
    /// </summary>
    int DisabledCount { get; }

    /// <summary>
    /// 验证总次数。
    /// </summary>
    long TotalValidations { get; }
    /// <summary>
    /// 验证成功次数。
    /// </summary>
    long SuccessfulValidations { get; }
    /// <summary>
    /// 验证失败次数。
    /// </summary>
    long FailedValidations { get; }

    /// <summary>
    /// 获取请求总数。
    /// </summary>
    long TotalGetRequests { get; }
    /// <summary>
    /// 获取成功总数。
    /// </summary>
    long SuccessfulGetRequests { get; }
    /// <summary>
    /// 等待获取总数。
    /// </summary>
    long WaitingGetRequests { get; }

    /// <summary>
    /// 平均验证耗时。
    /// </summary>
    double AvgValidationTime { get; }
    /// <summary>
    /// 平均响应时间。
    /// </summary>
    double AvgResponseTime { get; }
    /// <summary>
    /// 总体成功率。
    /// </summary>
    double OverallSuccessRate { get; }
}
