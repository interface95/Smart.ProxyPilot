using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyProvider
{
    string Name { get; }
    ValueTask<IEnumerable<ProxyInfo>> FetchAsync(int count, CancellationToken ct = default);
}
