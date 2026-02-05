using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Tests.Fixtures;

public class MockProxyValidator : IProxyValidator
{
    public Func<ProxyInfo, ValidationResult>? ValidateFunc { get; set; }

    public ValueTask<ValidationResult> ValidateAsync(ProxyInfo proxy, CancellationToken ct = default)
    {
        var result = ValidateFunc?.Invoke(proxy)
            ?? ValidationResult.Success(TimeSpan.FromMilliseconds(100), 200);
        return ValueTask.FromResult(result);
    }
}
