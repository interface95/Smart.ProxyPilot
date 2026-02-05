using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyPool : IAsyncDisposable
{
    /// <summary>
    /// 启动代理池。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>
    /// 停止代理池。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    Task StopAsync(CancellationToken ct = default);

    /// <summary>
    /// 尝试获取代理（不等待）。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>代理或 null。</returns>
    ValueTask<ProxyInfo?> TryGetProxyAsync(CancellationToken ct = default);

    /// <summary>
    /// 获取代理（可等待）。
    /// </summary>
    /// <param name="timeout">超时时间，null 使用默认值。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>代理。</returns>
    ValueTask<ProxyInfo> GetProxyAsync(TimeSpan? timeout = null, CancellationToken ct = default);

    /// <summary>
    /// 等待获取代理，超时返回 null。
    /// </summary>
    /// <param name="timeout">超时时间，null 使用默认值。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>代理或 null。</returns>
    ValueTask<ProxyInfo?> TryGetProxyOrDefaultAsync(TimeSpan? timeout = null, CancellationToken ct = default);

    /// <summary>
    /// 当前验证并发数。
    /// </summary>
    int CurrentValidationConcurrency { get; }

    /// <summary>
    /// 动态调整验证并发数。
    /// </summary>
    /// <param name="newConcurrency">新的并发数。</param>
    void UpdateValidationConcurrency(int newConcurrency);

    /// <summary>
    /// 上报代理使用成功。
    /// </summary>
    /// <param name="proxy">代理。</param>
    /// <param name="responseTime">响应时间。</param>
    void ReportSuccess(ProxyInfo proxy, TimeSpan? responseTime = null);

    /// <summary>
    /// 上报代理使用失败。
    /// </summary>
    /// <param name="proxy">代理。</param>
    /// <param name="reason">失败原因。</param>
    void ReportFailure(ProxyInfo proxy, string? reason = null);

    /// <summary>
    /// 获取当前状态对象（引用不变）。
    /// </summary>
    IProxyPoolState CurrentState { get; }

}
