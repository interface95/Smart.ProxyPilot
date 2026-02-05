using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyStorage
{
    /// <summary>
    /// 添加代理。
    /// </summary>
    /// <param name="proxy">代理信息。</param>
    /// <param name="ct">取消令牌。</param>
    ValueTask AddAsync(ProxyInfo proxy, CancellationToken ct = default);

    /// <summary>
    /// 根据 ID 获取代理。
    /// </summary>
    /// <param name="id">代理 ID。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>代理或 null。</returns>
    ValueTask<ProxyInfo?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// 更新代理。
    /// </summary>
    /// <param name="proxy">代理信息。</param>
    /// <param name="ct">取消令牌。</param>
    ValueTask UpdateAsync(ProxyInfo proxy, CancellationToken ct = default);

    /// <summary>
    /// 移除代理。
    /// </summary>
    /// <param name="id">代理 ID。</param>
    /// <param name="ct">取消令牌。</param>
    ValueTask RemoveAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// 按状态获取代理列表。
    /// </summary>
    /// <param name="state">代理状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>代理列表。</returns>
    ValueTask<IReadOnlyList<ProxyInfo>> GetByStateAsync(ProxyState state, CancellationToken ct = default);

    /// <summary>
    /// 获取池快照统计。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>池快照。</returns>
    ValueTask<ProxyPoolSnapshot> GetSnapshotAsync(CancellationToken ct = default);
}
