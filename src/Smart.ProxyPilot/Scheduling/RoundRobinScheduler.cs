using System.Threading;
using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Scheduling;

public class RoundRobinScheduler() : IProxyScheduler
{
    private int _index = -1;

    /// <summary>
    /// 按轮询选择代理。
    /// </summary>
    public ProxyInfo? Select(IReadOnlyList<ProxyInfo> proxies)
    {
        if (proxies.Count == 0)
        {
            return null;
        }

        var next = Interlocked.Increment(ref _index);
        return proxies[next % proxies.Count];
    }

    /// <summary>
    /// 使用结果回调（轮询无需处理）。
    /// </summary>
    public void OnProxyUsed(ProxyInfo proxy, bool success, TimeSpan? responseTime)
    {
    }
}
