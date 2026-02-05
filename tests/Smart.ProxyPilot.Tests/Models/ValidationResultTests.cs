using Smart.ProxyPilot.Models;
using Xunit;

namespace Smart.ProxyPilot.Tests.Models;

public class ValidationResultTests
{
    [Fact]
    public void SuccessFactory_ShouldSetFields()
    {
        var result = ValidationResult.Success(TimeSpan.FromMilliseconds(20), 200);
        Assert.True(result.IsSuccess);
        Assert.Equal(ValidationResultType.Success, result.ResultType);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void TimeoutFactory_ShouldSetFields()
    {
        var result = ValidationResult.Timeout(TimeSpan.FromMilliseconds(30));
        Assert.False(result.IsSuccess);
        Assert.Equal(ValidationResultType.Timeout, result.ResultType);
    }
}
