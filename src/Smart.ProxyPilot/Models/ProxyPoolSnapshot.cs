namespace Smart.ProxyPilot.Models;

public class ProxyPoolSnapshot()
{
    /// <summary>
    /// 快照时间。
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 总数。
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// 待验证数。
    /// </summary>
    public int PendingCount { get; set; }
    /// <summary>
    /// 验证中数量。
    /// </summary>
    public int ValidatingCount { get; set; }
    /// <summary>
    /// 可用数量。
    /// </summary>
    public int AvailableCount { get; set; }
    /// <summary>
    /// 使用中数量。
    /// </summary>
    public int InUseCount { get; set; }
    /// <summary>
    /// 冷却中数量。
    /// </summary>
    public int CooldownCount { get; set; }
    /// <summary>
    /// 禁用数量。
    /// </summary>
    public int DisabledCount { get; set; }

    /// <summary>
    /// 验证总次数。
    /// </summary>
    public long TotalValidations { get; set; }
    /// <summary>
    /// 验证成功次数。
    /// </summary>
    public long SuccessfulValidations { get; set; }
    /// <summary>
    /// 验证失败次数。
    /// </summary>
    public long FailedValidations { get; set; }

    /// <summary>
    /// 获取请求总数。
    /// </summary>
    public long TotalGetRequests { get; set; }
    /// <summary>
    /// 获取成功总数。
    /// </summary>
    public long SuccessfulGetRequests { get; set; }
    /// <summary>
    /// 等待获取总数。
    /// </summary>
    public long WaitingGetRequests { get; set; }

    /// <summary>
    /// 平均验证耗时。
    /// </summary>
    public double AvgValidationTime { get; set; }
    /// <summary>
    /// 平均响应时间。
    /// </summary>
    public double AvgResponseTime { get; set; }
    /// <summary>
    /// 总体成功率。
    /// </summary>
    public double OverallSuccessRate { get; set; }
}
