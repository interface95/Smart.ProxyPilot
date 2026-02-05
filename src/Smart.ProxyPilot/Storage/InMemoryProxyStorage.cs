using System.Collections.Concurrent;
using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Storage;

public class InMemoryProxyStorage() : IProxyStorage
{
    private readonly ConcurrentDictionary<string, ProxyInfo> _proxies = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 添加代理。
    /// </summary>
    public ValueTask AddAsync(ProxyInfo proxy, CancellationToken ct = default)
    {
        _proxies.TryAdd(proxy.Id, proxy);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 根据 ID 获取代理。
    /// </summary>
    public ValueTask<ProxyInfo?> GetByIdAsync(string id, CancellationToken ct = default)
        => ValueTask.FromResult(_proxies.TryGetValue(id, out var proxy) ? proxy : null);

    /// <summary>
    /// 更新代理。
    /// </summary>
    public ValueTask UpdateAsync(ProxyInfo proxy, CancellationToken ct = default)
    {
        _proxies[proxy.Id] = proxy;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 删除代理。
    /// </summary>
    public ValueTask RemoveAsync(string id, CancellationToken ct = default)
    {
        _proxies.TryRemove(id, out _);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 按状态获取代理列表。
    /// </summary>
    public ValueTask<IReadOnlyList<ProxyInfo>> GetByStateAsync(ProxyState state, CancellationToken ct = default)
    {
        var list = _proxies.Values.Where(p => p.State == state).ToList();
        return ValueTask.FromResult<IReadOnlyList<ProxyInfo>>(list);
    }

}
