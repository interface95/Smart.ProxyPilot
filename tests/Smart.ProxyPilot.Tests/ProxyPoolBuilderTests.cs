using Smart.ProxyPilot;
using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Options;
using Smart.ProxyPilot.Providers;
using Xunit;

namespace Smart.ProxyPilot.Tests;

public class ProxyPoolBuilderTests
{
    [Fact]
    public void AddApiProvider_ShouldBuildPool()
    {
        // Tests builder with API provider creates a pool.
        var builder = new ProxyPoolBuilder()
            .Configure(options => options.ValidationUrl = "https://httpbin.org/ip")
            .AddApiProvider(new Uri("http://example.com/api?qty=5"));

        var pool = builder.Build();
        Assert.NotNull(pool);
    }
}
