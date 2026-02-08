namespace Smart.ProxyPilot.Options;

public class HttpProxyValidatorOptions(Uri validationUrl)
{
    /// <summary>
    /// 验证地址。
    /// </summary>
    public Uri ValidationUrl { get; } = validationUrl;

    /// <summary>
    /// 验证超时。
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// 自定义响应校验函数。
    /// </summary>
    public Func<HttpResponseMessage, ValueTask<bool>>? ValidationFunc { get; set; }

    /// <summary>
    /// 期望状态码。
    /// </summary>
    public int ExpectedStatusCode { get; set; } = 200;

    /// <summary>
    /// 自定义 HttpClient（仅用于复用默认请求头）。
    /// </summary>
    public HttpClient? Client { get; set; }

    /// <summary>
    /// 缓存的代理请求器最大数量。
    /// </summary>
    public int MaxCachedProxyInvokers { get; set; } = 512;

    /// <summary>
    /// 缓存的代理请求器空闲回收时间。
    /// </summary>
    public TimeSpan CachedProxyInvokerIdleTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 连接池中连接的最大生命周期。
    /// </summary>
    public TimeSpan PooledConnectionLifetime { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 连接池中空闲连接的回收时间。
    /// </summary>
    public TimeSpan PooledConnectionIdleTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// 每个目标主机的最大并发连接数。
    /// </summary>
    public int MaxConnectionsPerServer { get; set; } = 4;
}
