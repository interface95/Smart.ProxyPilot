namespace Smart.ProxyPilot.Options;

public class ProxyPoolOptions()
{
    /// <summary>
    /// 验证超时时间。
    /// </summary>
    public TimeSpan ValidationTimeout { get; set; } = TimeSpan.FromSeconds(10);
    /// <summary>
    /// 验证并发数。
    /// </summary>
    public int ValidationConcurrency { get; set; } = 10;
    /// <summary>
    /// 验证目标 URL。
    /// </summary>
    public string ValidationUrl { get; set; } = "http://httpbin.org/ip";
    /// <summary>
    /// 自定义验证函数。
    /// </summary>
    public Func<HttpResponseMessage, ValueTask<bool>>? ValidationFunc { get; set; }
    /// <summary>
    /// 重新验证间隔。
    /// </summary>
    public TimeSpan ValidationInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 最小池大小。
    /// </summary>
    public int MinPoolSize { get; set; } = 5;
    /// <summary>
    /// 最大池大小。
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;
    /// <summary>
    /// 代理过期时间。
    /// </summary>
    public TimeSpan ProxyExpireTime { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// 连续失败上限。
    /// </summary>
    public int MaxConsecutiveFailCount { get; set; } = 3;
    /// <summary>
    /// 冷却时间。
    /// </summary>
    public TimeSpan CooldownDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 默认获取超时。
    /// </summary>
    public TimeSpan DefaultGetTimeout { get; set; } = TimeSpan.FromSeconds(30);
    /// <summary>
    /// 获取后是否移除。
    /// </summary>
    public bool RemoveAfterGet { get; set; } = false;

    /// <summary>
    /// 每次拉取数量。
    /// </summary>
    public int FetchBatchSize { get; set; } = 50;
    /// <summary>
    /// 拉取间隔。
    /// </summary>
    public TimeSpan FetchInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 可用代理达到该阈值后，新拉取的代理将不再进入验证队列。
    /// </summary>
    /// <remarks>
    /// null 或 &lt;= 0 表示不启用。
    /// </remarks>
    public int? MaxAvailableCountForValidation { get; set; }

    /// <summary>
    /// 事件回调接收器。
    /// </summary>
    public Abstractions.IProxyEventSink? EventSink { get; set; }
}
