using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Scheduling;
using Xunit;

namespace Smart.ProxyPilot.Tests.Schedulers;

public class WeightedSchedulerTests
{
    [Fact]
    public void Select_ShouldPreferHigherWeightProxy()
    {
        // Tests weighted selection favors higher weight.
        var scheduler = new WeightedScheduler();
        var fast = new ProxyInfo("1.1.1.1", 8080, ProxyType.Http);
        fast.Statistics.RecordValidation(ValidationResult.Success(TimeSpan.FromMilliseconds(50), 200));

        var slow = new ProxyInfo("2.2.2.2", 8080, ProxyType.Http);
        slow.Statistics.RecordValidation(ValidationResult.Success(TimeSpan.FromMilliseconds(500), 200));

        var proxies = new List<ProxyInfo> { fast, slow };
        var selections = Enumerable.Range(0, 1000)
            .Select(_ => scheduler.Select(proxies)!)
            .GroupBy(p => p.Host)
            .ToDictionary(g => g.Key, g => g.Count());

        Assert.True(selections["1.1.1.1"] > selections["2.2.2.2"]);
    }
}
