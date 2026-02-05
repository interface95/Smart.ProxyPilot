using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyValidator
{
    ValueTask<ValidationResult> ValidateAsync(ProxyInfo proxy, CancellationToken ct = default);
}
