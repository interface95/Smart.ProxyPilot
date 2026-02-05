using System.Net;

namespace Smart.ProxyPilot.Models;

public class ProxyInfo(string host, int port, ProxyType type)
{
    public string Host { get; set; } = string.IsNullOrWhiteSpace(host)
        ? throw new ArgumentException("Host is required.", nameof(host))
        : host;

    public int Port { get; set; } = port > 0
        ? port
        : throw new ArgumentOutOfRangeException(nameof(port), "Port must be positive.");

    public ProxyType Type { get; set; } = type;

    public string? Username { get; set; }
    public string? Password { get; set; }

    public string? Source { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public bool IsAnonymous { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }

    public ProxyState State { get; set; } = ProxyState.Pending;

    public ProxyStatistics Statistics { get; } = new();

    public string Id => $"{Host}:{Port}";

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
