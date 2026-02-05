using Smart.ProxyPilot;
using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Options;
using Smart.ProxyPilot.Scheduling;
using Smart.ProxyPilot.Storage;
using Smart.ProxyPilot.Tests.Fixtures;
using Xunit;

namespace Smart.ProxyPilot.Tests;

public class ProxyPoolTests
{
    [Fact]
    public async Task GetProxyAsync_ShouldWaitWhenPoolEmpty()
    {
        var provider = new MockProxyProvider();
        var validator = new MockProxyValidator();
        var options = new ProxyPoolOptions
        {
            FetchInterval = TimeSpan.FromMilliseconds(50),
            ValidationInterval = TimeSpan.FromMinutes(10),
            MinPoolSize = 1,
            FetchBatchSize = 1,
            ValidationConcurrency = 1
        };

        var pool = new ProxyPool(options, [provider], validator, new RoundRobinScheduler(), new InMemoryProxyStorage());
        await pool.StartAsync();

        var getTask = pool.GetProxyAsync(timeout: TimeSpan.FromSeconds(2));
        await Task.Delay(100);
        provider.AddProxy(new ProxyInfo("1.1.1.1", 8080, ProxyType.Http));

        var proxy = await getTask;
        Assert.NotNull(proxy);

        await pool.StopAsync();
    }

    [Fact]
    public async Task GetProxyAsync_ShouldThrowOnTimeout()
    {
        var provider = new MockProxyProvider();
        var validator = new MockProxyValidator();
        var options = new ProxyPoolOptions
        {
            FetchInterval = TimeSpan.FromSeconds(5),
            ValidationInterval = TimeSpan.FromMinutes(10),
            MinPoolSize = 1,
            FetchBatchSize = 1,
            ValidationConcurrency = 1
        };

        var pool = new ProxyPool(options, [provider], validator, new RoundRobinScheduler(), new InMemoryProxyStorage());
        await pool.StartAsync();

        await Assert.ThrowsAsync<TimeoutException>(() => pool.GetProxyAsync(timeout: TimeSpan.FromMilliseconds(200)).AsTask());

        await pool.StopAsync();
    }

    [Fact]
    public async Task TryGetProxyOrDefaultAsync_ShouldReturnNullOnTimeout()
    {
        // Timeout returns null instead of throwing.
        var provider = new MockProxyProvider();
        var validator = new MockProxyValidator();
        var options = new ProxyPoolOptions
        {
            FetchInterval = TimeSpan.FromSeconds(5),
            ValidationInterval = TimeSpan.FromMinutes(10),
            MinPoolSize = 1,
            FetchBatchSize = 1,
            ValidationConcurrency = 1
        };

        var pool = new ProxyPool(options, [provider], validator, new RoundRobinScheduler(), new InMemoryProxyStorage());
        await pool.StartAsync();

        var proxy = await pool.TryGetProxyOrDefaultAsync(timeout: TimeSpan.FromMilliseconds(200));

        Assert.Null(proxy);
        await pool.StopAsync();
    }

    [Fact]
    public async Task UpdateValidationConcurrency_ShouldAdjustWorkers()
    {
        var provider = new MockProxyProvider();
        var validator = new MockProxyValidator();
        var options = new ProxyPoolOptions
        {
            ValidationConcurrency = 2,
            FetchInterval = TimeSpan.FromSeconds(5),
            ValidationInterval = TimeSpan.FromMinutes(10),
            MinPoolSize = 1,
            FetchBatchSize = 1
        };

        var pool = new ProxyPool(options, [provider], validator, new RoundRobinScheduler(), new InMemoryProxyStorage());
        await pool.StartAsync();

        Assert.Equal(2, pool.CurrentValidationConcurrency);
        pool.UpdateValidationConcurrency(4);
        Assert.Equal(4, pool.CurrentValidationConcurrency);
        pool.UpdateValidationConcurrency(1);
        Assert.Equal(1, pool.CurrentValidationConcurrency);

        await pool.StopAsync();
    }
}
