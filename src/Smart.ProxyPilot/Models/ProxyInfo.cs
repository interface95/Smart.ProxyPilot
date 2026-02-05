using System.Net;

namespace Smart.ProxyPilot.Models;

public class ProxyInfo(string host, int port, ProxyType type)
{
    /// <summary>
    /// 主机地址。
    /// </summary>
    public string Host { get; set; } = string.IsNullOrWhiteSpace(host)
        ? throw new ArgumentException("Host is required.", nameof(host))
        : host;

    /// <summary>
    /// 端口。
    /// </summary>
    public int Port { get; set; } = port > 0
        ? port
        : throw new ArgumentOutOfRangeException(nameof(port), "Port must be positive.");

    /// <summary>
    /// 代理类型。
    /// </summary>
    public ProxyType Type { get; set; } = type;

    /// <summary>
    /// 认证用户名。
    /// </summary>
    public string? Username { get; set; }
    /// <summary>
    /// 认证密码。
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 来源标识。
    /// </summary>
    public string? Source { get; set; }
    /// <summary>
    /// 国家。
    /// </summary>
    public string? Country { get; set; }
    /// <summary>
    /// 地区。
    /// </summary>
    public string? Region { get; set; }
    /// <summary>
    /// 是否匿名。
    /// </summary>
    public bool IsAnonymous { get; set; }
    /// <summary>
    /// 自定义元数据。
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// 当前状态。
    /// </summary>
    public ProxyState State { get; set; } = ProxyState.Pending;

    /// <summary>
    /// 统计信息。
    /// </summary>
    public ProxyStatistics Statistics { get; } = new();

    /// <summary>
    /// 唯一标识（Host:Port）。
    /// </summary>
    public string Id => $"{Host}:{Port}";

    /// <summary>
    /// 计算权重。
    /// </summary>
    public double CalculateWeight()
    {
        var successRate = Math.Clamp(Statistics.ValidationSuccessRate, 0.01, 1.0);
        var responseTime = Statistics.AvgResponseTime <= 1 ? 1 : Statistics.AvgResponseTime;
        var freshnessPenalty = Statistics.LastUsedAt is null
            ? 1.0
            : 1.0 / (1.0 + Math.Min(60, (DateTime.UtcNow - Statistics.LastUsedAt.Value).TotalMinutes));
        return successRate * (1.0 / responseTime) * (0.5 + freshnessPenalty);
    }

    public WebProxy ToWebProxy()
    {
        var proxy = new WebProxy(ToUri());
        if (!string.IsNullOrWhiteSpace(Username))
        {
            proxy.Credentials = new NetworkCredential(Username, Password);
        }

        return proxy;
    }

    public Uri ToUri()
    {
        var scheme = Type switch
        {
            ProxyType.Http => "http",
            ProxyType.Https => "https",
            ProxyType.Socks4 => "socks4",
            ProxyType.Socks5 => "socks5",
            _ => "http"
        };

        return new Uri($"{scheme}://{Host}:{Port}");
    }

    public override string ToString() => $"{Type}://{Host}:{Port}";
}
