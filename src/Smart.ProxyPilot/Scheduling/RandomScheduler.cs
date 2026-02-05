using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Scheduling;

public class RandomScheduler() : IProxyScheduler
{
    /// <summary>
    /// 随机选择代理。
    /// </summary>
    public ProxyInfo? Select(IReadOnlyList<ProxyInfo> proxies)
    {
        if (proxies.Count == 0)
        {
            return null;
        }

        var index = Random.Shared.Next(0, proxies.Count);
        return proxies[index];
    }

    /// <summary>
    /// 使用结果回调（随机无需处理）。
    /// </summary>
    public void OnProxyUsed(ProxyInfo proxy, bool success, TimeSpan? responseTime)
    {
    }
}
