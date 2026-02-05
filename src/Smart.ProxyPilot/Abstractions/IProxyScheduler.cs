using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyScheduler
{
    ProxyInfo? Select(IReadOnlyList<ProxyInfo> proxies);
    void OnProxyUsed(ProxyInfo proxy, bool success, TimeSpan? responseTime);
}
