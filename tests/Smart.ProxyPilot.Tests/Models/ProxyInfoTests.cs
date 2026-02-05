using Smart.ProxyPilot.Models;
using Xunit;

namespace Smart.ProxyPilot.Tests.Models;

public class ProxyInfoTests
{
    [Fact]
    public void Id_ShouldBeHostPort()
    {
        // Tests Id formatting.
        var proxy = new ProxyInfo("1.1.1.1", 8080, ProxyType.Http);
        Assert.Equal("1.1.1.1:8080", proxy.Id);
    }

    [Fact]
    public void ToUri_ShouldUseScheme()
    {
        // Tests URI scheme mapping by type.
        var proxy = new ProxyInfo("example.com", 1080, ProxyType.Socks5);
        Assert.Equal("socks5://example.com:1080/", proxy.ToUri().ToString());
    }

    [Fact]
    public void CalculateWeight_ShouldReturnPositive()
    {
        // Tests weight calculation returns a positive value.
        var proxy = new ProxyInfo("1.1.1.1", 8080, ProxyType.Http);
        proxy.Statistics.RecordValidation(ValidationResult.Success(TimeSpan.FromMilliseconds(50), 200));
        Assert.True(proxy.CalculateWeight() > 0);
    }
}
