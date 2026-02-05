using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Scheduling;
using Xunit;

namespace Smart.ProxyPilot.Tests.Schedulers;

public class RandomSchedulerTests
{
    [Fact]
    public void Select_ShouldReturnFromList()
    {
        var scheduler = new RandomScheduler();
        var proxies = new List<ProxyInfo>
        {
            new("1.1.1.1", 8080, ProxyType.Http),
            new("2.2.2.2", 8080, ProxyType.Http)
        };

        var selected = scheduler.Select(proxies);
        Assert.NotNull(selected);
        Assert.Contains(selected!, proxies);
    }
}
