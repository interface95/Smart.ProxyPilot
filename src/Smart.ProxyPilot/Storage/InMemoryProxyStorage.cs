using System.Collections.Concurrent;
using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Storage;

public class InMemoryProxyStorage() : IProxyStorage
{
    private readonly ConcurrentDictionary<string, ProxyInfo> _proxies = new(StringComparer.OrdinalIgnoreCase);

    public ValueTask AddAsync(ProxyInfo proxy, CancellationToken ct = default)
    {
        _proxies.TryAdd(proxy.Id, proxy);
        return ValueTask.CompletedTask;
    }

    public ValueTask<ProxyInfo?> GetByIdAsync(string id, CancellationToken ct = default)
        => ValueTask.FromResult(_proxies.TryGetValue(id, out var proxy) ? proxy : null);

    public ValueTask UpdateAsync(ProxyInfo proxy, CancellationToken ct = default)
    {
        _proxies[proxy.Id] = proxy;
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(string id, CancellationToken ct = default)
    {
        _proxies.TryRemove(id, out _);
        return ValueTask.CompletedTask;
    }

    public ValueTask<IReadOnlyList<ProxyInfo>> GetByStateAsync(ProxyState state, CancellationToken ct = default)
    {
        var list = _proxies.Values.Where(p => p.State == state).ToList();
        return ValueTask.FromResult<IReadOnlyList<ProxyInfo>>(list);
    }

    public ValueTask<ProxyPoolSnapshot> GetSnapshotAsync(CancellationToken ct = default)
    {
        var proxies = _proxies.Values.ToList();
        var snapshot = new ProxyPoolSnapshot
        {
            TotalCount = proxies.Count,
            PendingCount = proxies.Count(p => p.State == ProxyState.Pending),
            ValidatingCount = proxies.Count(p => p.State == ProxyState.Validating),
            AvailableCount = proxies.Count(p => p.State == ProxyState.Available),
            InUseCount = proxies.Count(p => p.State == ProxyState.InUse),
            CooldownCount = proxies.Count(p => p.State == ProxyState.Cooldown),
            DisabledCount = proxies.Count(p => p.State == ProxyState.Disabled)
        };

        var totalValidations = proxies.Sum(p => p.Statistics.TotalValidationCount);
        var successValidations = proxies.Sum(p => p.Statistics.ValidationSuccessCount);
        var failedValidations = proxies.Sum(p => p.Statistics.ValidationFailCount);
        snapshot.TotalValidations = totalValidations;
        snapshot.SuccessfulValidations = successValidations;
        snapshot.FailedValidations = failedValidations;

        snapshot.AvgValidationTime = totalValidations > 0
            ? proxies.Sum(p => p.Statistics.AvgResponseTime * p.Statistics.TotalValidationCount) / totalValidations
            : 0;
        snapshot.AvgResponseTime = proxies.Count > 0
            ? proxies.Average(p => p.Statistics.AvgResponseTime)
            : 0;
        snapshot.OverallSuccessRate = totalValidations > 0
            ? (double)successValidations / totalValidations
            : 0;

        snapshot.TotalGetRequests = proxies.Sum(p => p.Statistics.TotalUseCount);
        snapshot.SuccessfulGetRequests = proxies.Sum(p => p.Statistics.UseSuccessCount);
        snapshot.WaitingGetRequests = 0;
        return ValueTask.FromResult(snapshot);
    }
}
