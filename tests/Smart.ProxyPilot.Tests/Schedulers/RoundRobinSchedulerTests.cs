using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Scheduling;
using Xunit;

namespace Smart.ProxyPilot.Tests.Schedulers;

public class RoundRobinSchedulerTests
{
    [Fact]
    public void Select_ShouldCycleProxies()
    {
        // Tests round-robin ordering.
        var scheduler = new RoundRobinScheduler();
        var proxies = new List<ProxyInfo>
        {
            new("1.1.1.1", 8080, ProxyType.Http),
            new("2.2.2.2", 8080, ProxyType.Http),
            new("3.3.3.3", 8080, ProxyType.Http)
        };

        var first = scheduler.Select(proxies);
        var second = scheduler.Select(proxies);
        var third = scheduler.Select(proxies);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotNull(third);
        Assert.NotEqual(first!.Host, second!.Host);
        Assert.NotEqual(second!.Host, third!.Host);
    }
}
