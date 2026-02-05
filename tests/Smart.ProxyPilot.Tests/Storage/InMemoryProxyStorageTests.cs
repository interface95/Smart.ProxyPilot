using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Storage;
using Xunit;

namespace Smart.ProxyPilot.Tests.Storage;

public class InMemoryProxyStorageTests
{
    [Fact]
    public async Task Crud_ShouldWork()
    {
        // Tests basic CRUD and state filtering.
        var storage = new InMemoryProxyStorage();
        var proxy = new ProxyInfo("1.1.1.1", 8080, ProxyType.Http);

        await storage.AddAsync(proxy);
        var fetched = await storage.GetByIdAsync(proxy.Id);
        Assert.NotNull(fetched);

        proxy.State = ProxyState.Available;
        await storage.UpdateAsync(proxy);
        var available = await storage.GetByStateAsync(ProxyState.Available);
        Assert.Single(available);

        await storage.RemoveAsync(proxy.Id);
        var afterRemove = await storage.GetByIdAsync(proxy.Id);
        Assert.Null(afterRemove);
    }

    
}
