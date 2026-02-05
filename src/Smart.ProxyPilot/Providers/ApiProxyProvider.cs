using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Options;
using Smart.ProxyPilot.Parsers;

namespace Smart.ProxyPilot.Providers;

public class ApiProxyProvider(ApiProxyProviderOptions options) : IProxyProvider, IDisposable
{
    private readonly HttpClient _client = options.Client ?? new HttpClient();
    private bool _disposed;

    public string Name => "Api";

    public async ValueTask<IEnumerable<ProxyInfo>> FetchAsync(int count, CancellationToken ct = default)
    {
        var content = await _client.GetStringAsync(options.ApiUrl, ct).ConfigureAwait(false);
        var parser = options.Parser ?? new LineProxyParser();
        var result = parser.Parse(content);
        return count > 0 ? result.Take(count).ToList() : result.ToList();
    }

    public void Dispose()
    {
        if (_disposed || options.Client is not null)
        {
            return;
        }

        _client.Dispose();
        _disposed = true;
    }
}
