using Smart.ProxyPilot.Abstractions;

namespace Smart.ProxyPilot.Options;

public class ApiProxyProviderOptions(Uri apiUrl)
{
    /// <summary>
    /// API 地址。
    /// </summary>
    public Uri ApiUrl { get; } = apiUrl;
    /// <summary>
    /// 内容解析器。
    /// </summary>
    public IProxyParser? Parser { get; set; }
    /// <summary>
    /// 自定义 HttpClient。
    /// </summary>
    public HttpClient? Client { get; set; }
    /// <summary>
    /// 批量数量（保留字段）。
    /// </summary>
    public int BatchSize { get; set; } = 50;
}
