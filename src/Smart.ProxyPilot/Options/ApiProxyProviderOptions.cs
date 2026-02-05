using Smart.ProxyPilot.Abstractions;

namespace Smart.ProxyPilot.Options;

public class ApiProxyProviderOptions(Uri apiUrl)
{
    public Uri ApiUrl { get; } = apiUrl;
    public IProxyParser? Parser { get; set; }
    public HttpClient? Client { get; set; }
    public int BatchSize { get; set; } = 50;
}
