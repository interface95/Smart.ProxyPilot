using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Providers;

public class CompositeProxyProvider(IEnumerable<IProxyProvider> providers) : IProxyProvider
{
    private readonly IReadOnlyList<IProxyProvider> _providers = providers.ToList();

    /// <summary>
    /// 代理源名称。
    /// </summary>
    public string Name => "Composite";

    /// <summary>
    /// 从多个代理源合并获取代理列表。
    /// </summary>
    /// <param name="count">数量，<=0 表示尽量返回所有结果。</param>
    /// <param name="ct">取消令牌。</param>
    public async ValueTask<IEnumerable<ProxyInfo>> FetchAsync(int count, CancellationToken ct = default)
    {
        if (_providers.Count == 0)
        {
            return [];
        }

        var results = new List<ProxyInfo>();
        foreach (var provider in _providers)
        {
            var items = await provider.FetchAsync(count, ct).ConfigureAwait(false);
            results.AddRange(items);
        }

        return results
            .GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .Take(count > 0 ? count : results.Count)
            .ToList();
    }
}
