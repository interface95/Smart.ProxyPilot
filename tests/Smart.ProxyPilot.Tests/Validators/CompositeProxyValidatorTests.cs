using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Tests.Fixtures;
using Smart.ProxyPilot.Validators;
using Xunit;

namespace Smart.ProxyPilot.Tests.Validators;

public class CompositeProxyValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldReturnFailureIfAnyValidatorFails()
    {
        // Tests composite failure propagation.
        var proxy = new ProxyInfo("1.1.1.1", 8080, ProxyType.Http);
        var validator1 = new MockProxyValidator
        {
            ValidateFunc = _ => ValidationResult.Success(TimeSpan.FromMilliseconds(10), 200)
        };
        var validator2 = new MockProxyValidator
        {
            ValidateFunc = _ => ValidationResult.Failed(ValidationResultType.InvalidResponse, "fail")
        };

        var composite = new CompositeProxyValidator([validator1, validator2]);
        var result = await composite.ValidateAsync(proxy);

        Assert.False(result.IsSuccess);
        Assert.Equal(ValidationResultType.InvalidResponse, result.ResultType);
    }
}
