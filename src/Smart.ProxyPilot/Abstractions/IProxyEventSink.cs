using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyEventSink
{
    /// <summary>
    /// 代理验证完成回调。
    /// </summary>
    /// <param name="proxy">代理。</param>
    /// <param name="result">验证结果。</param>
    void OnProxyValidated(ProxyInfo proxy, ValidationResult result);

    /// <summary>
    /// 代理状态变更回调。
    /// </summary>
    /// <param name="proxy">代理。</param>
    /// <param name="oldState">旧状态。</param>
    /// <param name="newState">新状态。</param>
    void OnProxyStateChanged(ProxyInfo proxy, ProxyState oldState, ProxyState newState);

    /// <summary>
    /// 池状态变更回调。
    /// </summary>
    /// <param name="state">池状态。</param>
    void OnPoolStateChanged(IProxyPoolState state);
}
