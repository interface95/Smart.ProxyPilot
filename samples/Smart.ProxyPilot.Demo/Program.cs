using System.Diagnostics;
using Smart.ProxyPilot;
using Smart.ProxyPilot.Options;
using Smart.ProxyPilot.Scheduling;
using Smart.ProxyPilot.Validators;

var pool = new ProxyPoolBuilder()
    .Configure(options =>
    {
        options.ValidationTimeout = TimeSpan.FromSeconds(5);
        options.ValidationConcurrency = 10;
        options.ValidationUrl = "https://httpbin.org/ip";
        options.MinPoolSize = 5;
        options.MaxConsecutiveFailCount = 3;
        options.CooldownDuration = TimeSpan.FromSeconds(30);
    })
    .AddApiProvider(new Uri("http://bapi.51daili.com/getapi2?linePoolIndex=-1&packid=2&time=1&qty=5&port=1&format=txt&usertype=17&uid=38584"))
    .UseValidator(new HttpProxyValidator(new HttpProxyValidatorOptions(new Uri("https://httpbin.org/ip"))))
    .UseScheduler(new WeightedScheduler())
    .Build();

pool.ProxyValidated += (_, e) =>
{
    Console.WriteLine($"[验证] {e.Proxy} - {e.Result.ResultType} ({e.Result.ResponseTime.TotalMilliseconds:0}ms)");
};

pool.ProxyStateChanged += (_, e) =>
{
    Console.WriteLine($"[状态] {e.Proxy} - {e.OldState} -> {e.NewState}");
};

await pool.StartAsync();

var snapshot = pool.GetSnapshot();
Console.WriteLine($"可用: {snapshot.AvailableCount}, 验证中: {snapshot.ValidatingCount}");

var proxy = await pool.TryGetProxyAsync();
if (proxy is not null)
{
    var sw = Stopwatch.StartNew();
    try
    {
        using var handler = new HttpClientHandler { Proxy = proxy.ToWebProxy(), UseProxy = true };
        using var client = new HttpClient(handler);
        await client.GetAsync("https://httpbin.org/ip");
        sw.Stop();
        pool.ReportSuccess(proxy, sw.Elapsed);
    }
    catch (Exception ex)
    {
        sw.Stop();
        pool.ReportFailure(proxy, ex.Message);
    }
}

await pool.StopAsync();
await pool.DisposeAsync();
