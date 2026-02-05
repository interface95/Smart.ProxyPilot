using Smart.ProxyPilot.Models;
using Xunit;

namespace Smart.ProxyPilot.Tests.Models;

public class ProxyStatisticsTests
{
    [Fact]
    public void RecordValidation_ShouldUpdateCounts()
    {
        var stats = new ProxyStatistics();
        stats.RecordValidation(ValidationResult.Success(TimeSpan.FromMilliseconds(10), 200));
        stats.RecordValidation(ValidationResult.Timeout(TimeSpan.FromMilliseconds(5)));

        Assert.Equal(2, stats.TotalValidationCount);
        Assert.Equal(1, stats.ValidationSuccessCount);
        Assert.Equal(1, stats.ValidationFailCount);
        Assert.Equal(1, stats.ValidationTimeoutCount);
    }

    [Fact]
    public void RecordUse_ShouldUpdateCounts()
    {
        var stats = new ProxyStatistics();
        stats.RecordUse(true, TimeSpan.FromMilliseconds(15), ValidationResultType.Success);
        stats.RecordUse(false, null, ValidationResultType.Timeout);

        Assert.Equal(2, stats.TotalUseCount);
        Assert.Equal(1, stats.UseSuccessCount);
        Assert.Equal(1, stats.UseFailCount);
        Assert.Equal(1, stats.UseTimeoutCount);
    }
}
