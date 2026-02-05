using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyScheduler
{
    /// <summary>
    /// 从可用代理中选择一个。
    /// </summary>
    /// <param name="proxies">可用代理列表。</param>
    /// <returns>选中的代理，或 null。</returns>
    ProxyInfo? Select(IReadOnlyList<ProxyInfo> proxies);

    /// <summary>
    /// 代理使用结果回调。
    /// </summary>
    /// <param name="proxy">已使用的代理。</param>
    /// <param name="success">是否成功。</param>
    /// <param name="responseTime">响应时间。</param>
    void OnProxyUsed(ProxyInfo proxy, bool success, TimeSpan? responseTime);
}
