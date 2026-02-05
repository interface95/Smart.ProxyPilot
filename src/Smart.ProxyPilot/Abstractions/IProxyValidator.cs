using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyValidator
{
    /// <summary>
    /// 验证代理可用性。
    /// </summary>
    /// <param name="proxy">待验证的代理。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>验证结果。</returns>
    ValueTask<ValidationResult> ValidateAsync(ProxyInfo proxy, CancellationToken ct = default);
}
