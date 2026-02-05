using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Tests.Fixtures;

public class MockProxyProvider(IEnumerable<ProxyInfo>? proxies = null) : IProxyProvider
{
    private readonly List<ProxyInfo> _proxies = proxies?.ToList() ?? [];
    public string Name => "Mock";

    public void AddProxy(ProxyInfo proxy) => _proxies.Add(proxy);

    public ValueTask<IEnumerable<ProxyInfo>> FetchAsync(int count, CancellationToken ct = default)
    {
        var result = count > 0 ? _proxies.Take(count).ToList() : _proxies.ToList();
        return ValueTask.FromResult<IEnumerable<ProxyInfo>>(result);
    }
}
