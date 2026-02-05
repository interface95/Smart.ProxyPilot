using System.Threading;
using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Scheduling;

public class RoundRobinScheduler() : IProxyScheduler
{
    private int _index = -1;

    public ProxyInfo? Select(IReadOnlyList<ProxyInfo> proxies)
    {
        if (proxies.Count == 0)
        {
            return null;
        }

        var next = Interlocked.Increment(ref _index);
        return proxies[next % proxies.Count];
    }

    public void OnProxyUsed(ProxyInfo proxy, bool success, TimeSpan? responseTime)
    {
    }
}
