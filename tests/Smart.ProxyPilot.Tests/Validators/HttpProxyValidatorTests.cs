using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Options;
using Smart.ProxyPilot.Validators;
using Xunit;

namespace Smart.ProxyPilot.Tests.Validators;

public class HttpProxyValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldReturnFailureForInvalidProxy()
    {
        var options = new HttpProxyValidatorOptions(new Uri("https://example.com"))
        {
            Timeout = TimeSpan.FromMilliseconds(200)
        };
        var validator = new HttpProxyValidator(options);
        var proxy = new ProxyInfo("127.0.0.1", 1, ProxyType.Http);

        var result = await validator.ValidateAsync(proxy);

        Assert.False(result.IsSuccess);
    }
}
