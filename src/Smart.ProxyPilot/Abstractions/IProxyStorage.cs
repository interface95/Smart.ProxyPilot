using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyStorage
{
    ValueTask AddAsync(ProxyInfo proxy, CancellationToken ct = default);
    ValueTask<ProxyInfo?> GetByIdAsync(string id, CancellationToken ct = default);
    ValueTask UpdateAsync(ProxyInfo proxy, CancellationToken ct = default);
    ValueTask RemoveAsync(string id, CancellationToken ct = default);
    ValueTask<IReadOnlyList<ProxyInfo>> GetByStateAsync(ProxyState state, CancellationToken ct = default);
    ValueTask<ProxyPoolSnapshot> GetSnapshotAsync(CancellationToken ct = default);
}
