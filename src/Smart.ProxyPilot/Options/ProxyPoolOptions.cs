namespace Smart.ProxyPilot.Options;

public class ProxyPoolOptions()
{
    public TimeSpan ValidationTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public int ValidationConcurrency { get; set; } = 10;
    public string ValidationUrl { get; set; } = "http://httpbin.org/ip";
    public Func<HttpResponseMessage, ValueTask<bool>>? ValidationFunc { get; set; }
    public TimeSpan ValidationInterval { get; set; } = TimeSpan.FromMinutes(5);

    public int MinPoolSize { get; set; } = 5;
    public int MaxPoolSize { get; set; } = 100;
    public TimeSpan ProxyExpireTime { get; set; } = TimeSpan.FromHours(1);

    public int MaxConsecutiveFailCount { get; set; } = 3;
    public TimeSpan CooldownDuration { get; set; } = TimeSpan.FromMinutes(1);

    public TimeSpan DefaultGetTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool RemoveAfterGet { get; set; } = false;

    public int FetchBatchSize { get; set; } = 50;
    public TimeSpan FetchInterval { get; set; } = TimeSpan.FromMinutes(1);
}
