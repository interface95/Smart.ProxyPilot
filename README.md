# Smart.ProxyPilot

Smart.ProxyPilot is an intelligent HTTP proxy pool framework for .NET.

Smart.ProxyPilot 是一个面向 .NET 的智能 HTTP 代理池框架。

## Features / 功能特性

- Providers / 代理来源
  - API provider: fetch proxies from an HTTP API (URL is used as-is).
  - File provider: fetch proxies from a local file.
  - Composite provider: aggregate multiple providers.

- Parsers / 内容解析
  - Pluggable via `IProxyParser`.
  - Default parser (`LineProxyParser`) parses plain text with one `ip:port` per line.
    - Supports both `\n` and `\r\n`.
    - Ignores empty lines and comment lines starting with `#`.

- Validation / 可用性验证
  - Concurrent validation using `Channel + worker pool` (worker count == validation concurrency).
  - Runtime concurrency tuning via `UpdateValidationConcurrency(int)`.
  - Built-in validators: `HttpProxyValidator`, `CompositeProxyValidator`.
  - HTTP validation (`HttpProxyValidator`): sends a `GET` request to `ValidationUrl` through the proxy.
    - Timeout is configurable.
    - Optional custom `ValidationFunc` for response checks.
    - Uses cached `HttpMessageInvoker` per proxy (backed by `SocketsHttpHandler`) to reduce socket/port pressure.

- Scheduling / 调度策略
  - Pluggable via `IProxyScheduler`.
  - Built-in schedulers: `WeightedScheduler`, `RoundRobinScheduler`, `RandomScheduler`.
  - Feedback loop: `ReportSuccess` / `ReportFailure` updates proxy statistics and scheduler weights.

- Storage / 存储
  - Default: `InMemoryProxyStorage`.
  - Pluggable via `IProxyStorage`.
  - Duplicate filtering: fetched proxies are deduplicated by `ProxyInfo.Id` before adding to storage.

- Pool lifecycle & capacity / 生命周期与容量控制
  - Start/stop: `StartAsync` / `StopAsync`.
  - Capacity limit: `MaxPoolSize`.
  - Fetch policy: `FetchBatchSize` + `FetchInterval`.
  - Expiration: `ProxyExpireTime`.
  - Failure policy: `MaxConsecutiveFailCount` + `CooldownDuration`.
  - Revalidation loop: re-validates `Available` proxies based on `ValidationInterval`.
  - New-fetch validation gating: `MaxAvailableCountForValidation` can pause enqueuing validation for newly fetched proxies when enough proxies are already available.

- Getting proxies / 获取代理
  - `TryGetProxyAsync()` returns immediately (or `null`).
  - `GetProxyAsync(timeout)` waits and throws `TimeoutException` on timeout.
  - `TryGetProxyOrDefaultAsync(timeout)` waits and returns `null` on timeout.
  - `RemoveAfterGet` controls whether a proxy is removed from the pool after being acquired.

- Observability / 状态与观测
  - `CurrentState` returns a stable reference (`IProxyPoolState`) that is updated internally (no frequent snapshot allocations).
  - Optional callback sink via `IProxyEventSink`.

## Target Framework / 目标框架

- `net10.0`.

## Installation / 安装

### NuGet

After the package is published:

```bash
dotnet add package Smart.ProxyPilot
```

### Project reference

```bash
dotnet add <your-project> reference src/Smart.ProxyPilot/Smart.ProxyPilot.csproj
```

## Quick Start / 快速开始

Example: fetch proxies from an API returning plain text `ip:port` (one per line) and validate against a URL.

```csharp
using Smart.ProxyPilot;
using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Options;

sealed class ConsoleProxyEventSink : IProxyEventSink
{
    public void OnProxyValidated(ProxyInfo proxy, ValidationResult result)
        => Console.WriteLine($"[Validate] {proxy} => {result.ResultType} ({result.ResponseTime.TotalMilliseconds:0}ms)");

    public void OnProxyStateChanged(ProxyInfo proxy, ProxyState oldState, ProxyState newState)
        => Console.WriteLine($"[State] {proxy} {oldState} -> {newState}");

    public void OnPoolStateChanged(IProxyPoolState state)
        => Console.WriteLine($"[Pool] total={state.TotalCount} available={state.AvailableCount} pending={state.PendingCount}");
}

var pool = new ProxyPoolBuilder()
    .Configure(o =>
    {
        o.ValidationUrl = "https://httpbin.org/ip";
        o.ValidationTimeout = TimeSpan.FromSeconds(5);
        o.ValidationConcurrency = 10;

        o.MaxPoolSize = 200;

        // When Available proxies reach this threshold, newly fetched proxies won't be enqueued for validation.
        // (Only affects new fetch; revalidation/cooldown still enqueue.)
        o.MaxAvailableCountForValidation = 50;

        o.EventSink = new ConsoleProxyEventSink();
    })
    .AddApiProvider(new Uri(
        "http://bapi.51daili.com/getapi2?linePoolIndex=-1&packid=2&time=1&qty=5&port=1&format=txt&usertype=17&uid=38584"))
    .Build();

await pool.StartAsync();

var proxy = await pool.TryGetProxyOrDefaultAsync(TimeSpan.FromSeconds(10));
if (proxy is not null)
{
    // Use proxy...
    pool.ReportSuccess(proxy);
}

// Read current state (stable reference)
var state = pool.CurrentState;
Console.WriteLine($"Available now: {state.AvailableCount}");

// Dynamic concurrency tuning
pool.UpdateValidationConcurrency(20);
Console.WriteLine($"Validation workers: {pool.CurrentValidationConcurrency}");

await pool.StopAsync();
await pool.DisposeAsync();
```

## Proxy Validation Details / 验证细节

- Validation URL / 验证地址
  - Controlled by `ProxyPoolOptions.ValidationUrl`.
  - `HttpProxyValidator` sends a `GET` request to this URL through the proxy.

- Validation timeout / 验证超时
  - Controlled by `ProxyPoolOptions.ValidationTimeout` (default 10s).

- What is considered "valid" / 验证成功判定
  - If `ValidationFunc` is provided, it decides the result.
  - Otherwise, `HttpProxyValidatorOptions.ExpectedStatusCode` (default 200) is used.

## NuGet Publishing (GitHub Actions) / NuGet 自动发布

This repo contains a workflow that packs and pushes to NuGet.

- Required secret: `NUGET_API_KEY`
- Publish by tag:

```bash
git tag v1.0.0
git push origin v1.0.0
```

Or run the workflow manually with an explicit `version` input.

## Tests / 测试

```bash
dotnet test Smart.ProxyPilot.sln
```

Note: one test may call the real proxy API URL. If you want fully offline tests, guard/skip it.

## Demo / 示例

See `samples/Smart.ProxyPilot.Demo/Program.cs`.
