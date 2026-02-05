namespace Smart.ProxyPilot.UI.Models;

/// <summary>
/// 代理状态枚举（UI 层独立定义，后续与核心库对接）
/// </summary>
public enum ProxyState
{
    /// <summary>可用</summary>
    Available,
    /// <summary>验证中</summary>
    Validating,
    /// <summary>冷却中</summary>
    Cooldown,
    /// <summary>已禁用</summary>
    Disabled
}

/// <summary>
/// 代理统计信息
/// </summary>
public record ProxyStatistics
{
    public int ValidationSuccessCount { get; init; }
    public int ValidationFailureCount { get; init; }
    public int ValidationTimeoutCount { get; init; }
    public int UseSuccessCount { get; init; }
    public int UseFailureCount { get; init; }
    public double AvgResponseTime { get; init; }
    public double MinResponseTime { get; init; }
    public double MaxResponseTime { get; init; }
    public int ConsecutiveSuccessCount { get; init; }
    public int ConsecutiveFailureCount { get; init; }
    public DateTime? LastValidationTime { get; init; }
    public DateTime? LastUseTime { get; init; }
}

/// <summary>
/// 代理信息（UI 层模型，后续与核心库对接）
/// </summary>
public record ProxyInfo
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public string ProxyType { get; init; } = "HTTP";
    public ProxyState State { get; init; } = ProxyState.Available;
    public ProxyStatistics Statistics { get; init; } = new();

    public string Address => $"{Host}:{Port}";
}

/// <summary>
/// 日志条目
/// </summary>
public record LogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public required string Category { get; init; }
    public required string Message { get; init; }
    public LogLevel Level { get; init; } = LogLevel.Info;
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}
