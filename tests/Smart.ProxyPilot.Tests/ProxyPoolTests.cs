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
        // Tests waiting for a proxy when pool is empty.
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
        // Tests timeout throws exception.
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
        // Tests dynamic validation worker count update.
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

    [Fact]
    public async Task Fetch_ShouldNotEnqueueValidation_WhenAvailableCountReachedThreshold()
    {
        var availableProxy = new ProxyInfo("9.9.9.9", 8080, ProxyType.Http)
        {
            State = ProxyState.Available
        };
        availableProxy.Statistics.RecordValidation(ValidationResult.Success(TimeSpan.FromMilliseconds(5), 200));

        var provider = new MockProxyProvider([
            new ProxyInfo("1.1.1.1", 8080, ProxyType.Http),
            new ProxyInfo("2.2.2.2", 8080, ProxyType.Http)
        ]);

        var validator = new MockProxyValidator
        {
            ValidateFunc = _ => ValidationResult.Success(TimeSpan.FromMilliseconds(10), 200)
        };

        var storage = new InMemoryProxyStorage();
        await storage.AddAsync(availableProxy);

        var options = new ProxyPoolOptions
        {
            MaxPoolSize = 10,
            FetchBatchSize = 2,
            FetchInterval = TimeSpan.FromMilliseconds(20),
            ValidationInterval = TimeSpan.FromMinutes(10),
            MaxAvailableCountForValidation = 1,
            ValidationConcurrency = 1
        };

        var pool = new ProxyPool(options, [provider], validator, new RoundRobinScheduler(), storage);
        await pool.StartAsync();

        await Task.Delay(150);

        var fetched1 = await storage.GetByIdAsync("1.1.1.1:8080");
        var fetched2 = await storage.GetByIdAsync("2.2.2.2:8080");
        Assert.NotNull(fetched1);
        Assert.NotNull(fetched2);
        Assert.Equal(ProxyState.Pending, fetched1!.State);
        Assert.Equal(ProxyState.Pending, fetched2!.State);

        await pool.StopAsync();
    }
}
