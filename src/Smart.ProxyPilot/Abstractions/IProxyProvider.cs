using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyProvider
{
    /// <summary>
    /// 代理源名称。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取指定数量的代理。
    /// </summary>
    /// <param name="count">数量，<=0 表示由实现自行决定。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>代理列表。</returns>
    ValueTask<IEnumerable<ProxyInfo>> FetchAsync(int count, CancellationToken ct = default);
}
