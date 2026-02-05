# Smart.ProxyPilot

Smart.ProxyPilot is an intelligent HTTP proxy pool framework. It supports fetching proxies from APIs or files, concurrent validation, scheduling, and rich statistics.

Smart.ProxyPilot 是一个智能 HTTP 代理池框架，支持 API/文件获取代理、多线程验证、调度策略与统计快照。

## Features / 功能特性
- Proxy pool lifecycle: start/stop, snapshot, events.
- Providers: API, File, Composite.
- Validators: HTTP, Composite.
- Schedulers: Weighted, RoundRobin, Random.
- Storage: In-memory (extensible).
- Dynamic validation concurrency (worker pool).
- Parsers: pluggable via `IProxyParser` (default line-based `ip:port`).

## Target Framework / 目标框架
- .NET 10 (`net10.0`).

## Installation / 安装
Reference the project in your solution:
```
dotnet add <your-project> reference src/Smart.ProxyPilot/Smart.ProxyPilot.csproj
```

## Quick Start / 快速开始
```csharp
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
    })
    .AddApiProvider(new Uri("http://bapi.51daili.com/getapi2?linePoolIndex=-1&packid=2&time=1&qty=5&port=1&format=txt&usertype=17&uid=38584"))
    .UseValidator(new HttpProxyValidator(new HttpProxyValidatorOptions(new Uri("https://httpbin.org/ip"))))
    .UseScheduler(new WeightedScheduler())
    .Build();

pool.ProxyValidated += (_, e) =>
{
    Console.WriteLine($"[Validate] {e.Proxy} - {e.Result.ResultType}");
};

await pool.StartAsync();

var proxy = await pool.TryGetProxyOrDefaultAsync(TimeSpan.FromSeconds(5));
if (proxy is not null)
{
    // Use proxy...
    pool.ReportSuccess(proxy);
}

await pool.StopAsync();
await pool.DisposeAsync();
```

## Core API / 核心 API
- `TryGetProxyAsync()` returns immediately or `null`.
- `GetProxyAsync(timeout)` waits and throws `TimeoutException` on timeout.
- `TryGetProxyOrDefaultAsync(timeout)` waits and returns `null` on timeout.
- `UpdateValidationConcurrency(int)` changes worker count at runtime.
- `GetSnapshot()` returns aggregated pool statistics.

## Dynamic Validation Concurrency / 动态验证并发
Validation runs on a fixed worker pool. The worker count equals the concurrency.
```csharp
pool.UpdateValidationConcurrency(20);
Console.WriteLine(pool.CurrentValidationConcurrency);
```

## Providers and Parsers / 代理源与解析器
### API Provider (default line parser)
Default parser reads `ip:port` per line.
```csharp
pool.AddApiProvider(new Uri("http://example.com/proxies?qty=5"));
```

### Custom Parser
Implement `IProxyParser` to handle different API formats.
```csharp
using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

public class JsonProxyParser : IProxyParser
{
    public IEnumerable<ProxyInfo> Parse(string content)
    {
        // parse JSON and yield ProxyInfo
        return [];
    }
}

pool.AddApiProvider(new Uri("http://example.com/json"), new JsonProxyParser());
```

## Validation / 验证
HTTP validator checks the response and optionally uses a custom validation function.
```csharp
var validator = new HttpProxyValidator(new HttpProxyValidatorOptions(new Uri("https://httpbin.org/ip"))
{
    Timeout = TimeSpan.FromSeconds(3)
});
```

## Tests / 测试
Run tests:
```
dotnet test Smart.ProxyPilot.sln
```
Note: one test hits the real API URL. If you want fully offline tests, remove or guard that test.

## Demo / 示例
See `samples/Smart.ProxyPilot.Demo/Program.cs`.
