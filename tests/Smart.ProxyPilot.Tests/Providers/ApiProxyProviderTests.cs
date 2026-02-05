using System.Net;
using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Options;
using Smart.ProxyPilot.Providers;
using Xunit;

namespace Smart.ProxyPilot.Tests.Providers;

public class ApiProxyProviderTests
{
    [Fact]
    public async Task FetchAsync_ShouldParseLinesByDefault()
    {
        var handler = new StubMessageHandler("1.1.1.1:8080\n2.2.2.2:9090\n");
        var client = new HttpClient(handler);
        var options = new ApiProxyProviderOptions(new Uri("http://example.com")) { Client = client };
        var provider = new ApiProxyProvider(options);

        var result = (await provider.FetchAsync(0)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("1.1.1.1:8080", result[0].Id);
        Assert.Equal(ProxyType.Http, result[0].Type);
    }

    [Fact]
    public async Task FetchAsync_ShouldUseUrlAsIs()
    {
        var handler = new StubMessageHandler("1.1.1.1:8080");
        var client = new HttpClient(handler);
        var url = new Uri("http://example.com/api?qty=5");
        var options = new ApiProxyProviderOptions(url) { Client = client };
        var provider = new ApiProxyProvider(options);

        _ = await provider.FetchAsync(5);

        Assert.Equal(url, handler.LastRequestUri);
    }

    [Fact]
    public async Task FetchAsync_ShouldWorkWithRealApi()
    {
        var url = new Uri("http://bapi.51daili.com/getapi2?linePoolIndex=-1&packid=2&time=1&qty=5&port=1&format=txt&usertype=17&uid=38584");
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        var options = new ApiProxyProviderOptions(url) { Client = client };
        var provider = new ApiProxyProvider(options);

        var result = (await provider.FetchAsync(0)).ToList();

        Assert.NotEmpty(result);
        Assert.All(result, proxy => Assert.False(string.IsNullOrWhiteSpace(proxy.Host)));
    }

    private sealed class StubMessageHandler(string content) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };
            return Task.FromResult(response);
        }
    }
}
