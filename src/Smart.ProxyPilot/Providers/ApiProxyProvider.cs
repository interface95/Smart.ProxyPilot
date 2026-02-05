using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Options;
using Smart.ProxyPilot.Parsers;

namespace Smart.ProxyPilot.Providers;

public class ApiProxyProvider(ApiProxyProviderOptions options) : IProxyProvider, IDisposable
{
    private readonly HttpClient _client = options.Client ?? new HttpClient();
    private bool _disposed;

    /// <summary>
    /// 代理源名称。
    /// </summary>
    public string Name => "Api";

    /// <summary>
    /// 从 API 获取代理列表。
    /// </summary>
    /// <param name="count">数量，<=0 表示返回全部解析结果。</param>
    /// <param name="ct">取消令牌。</param>
    public async ValueTask<IEnumerable<ProxyInfo>> FetchAsync(int count, CancellationToken ct = default)
    {
        var content = await _client.GetStringAsync(options.ApiUrl, ct).ConfigureAwait(false);
        var parser = options.Parser ?? new LineProxyParser();
        var result = parser.Parse(content);
        return count > 0 ? result.Take(count).ToList() : result.ToList();
    }

    /// <summary>
    /// 释放资源。
    /// </summary>
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
