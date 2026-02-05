using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Scheduling;

public class RandomScheduler() : IProxyScheduler
{
    public ProxyInfo? Select(IReadOnlyList<ProxyInfo> proxies)
    {
        if (proxies.Count == 0)
        {
            return null;
        }

        var index = Random.Shared.Next(0, proxies.Count);
        return proxies[index];
    }

    public void OnProxyUsed(ProxyInfo proxy, bool success, TimeSpan? responseTime)
    {
    }
}
