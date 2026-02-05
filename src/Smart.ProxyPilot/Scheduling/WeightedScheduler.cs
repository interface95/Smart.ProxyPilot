using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Scheduling;

public class WeightedScheduler() : IProxyScheduler
{
    /// <summary>
    /// 按权重选择代理。
    /// </summary>
    public ProxyInfo? Select(IReadOnlyList<ProxyInfo> proxies)
    {
        if (proxies.Count == 0)
        {
            return null;
        }

        var weights = proxies.Select(p => Math.Max(0.0001, p.CalculateWeight())).ToArray();
        var total = weights.Sum();
        var roll = Random.Shared.NextDouble() * total;
        var accum = 0d;

        for (var i = 0; i < weights.Length; i++)
        {
            accum += weights[i];
            if (roll <= accum)
            {
                return proxies[i];
            }
        }

        return proxies[^1];
    }

    /// <summary>
    /// 使用结果回调（权重由代理自身统计计算）。
    /// </summary>
    public void OnProxyUsed(ProxyInfo proxy, bool success, TimeSpan? responseTime)
    {
    }
}
