using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Scheduling;

public class WeightedScheduler() : IProxyScheduler
{
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

    public void OnProxyUsed(ProxyInfo proxy, bool success, TimeSpan? responseTime)
    {
    }
}
