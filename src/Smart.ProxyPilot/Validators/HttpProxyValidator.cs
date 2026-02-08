using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Options;

namespace Smart.ProxyPilot.Validators;

public sealed class HttpProxyValidator(HttpProxyValidatorOptions options) : IProxyValidator, IDisposable
{
    private sealed class CachedInvoker(HttpMessageInvoker invoker)
    {
        public HttpMessageInvoker Invoker { get; } = invoker;
        public long LastUsedTicks;

        public void Touch() => LastUsedTicks = DateTime.UtcNow.Ticks;

        public bool IsIdle(TimeSpan idleTimeout)
        {
            var last = new DateTime(Interlocked.Read(ref LastUsedTicks), DateTimeKind.Utc);
            return DateTime.UtcNow - last >= idleTimeout;
        }

        public void Dispose() => Invoker.Dispose();
    }

    private readonly ConcurrentDictionary<string, CachedInvoker> _invokers = new(StringComparer.OrdinalIgnoreCase);
    private long _lastCleanupTicks;

    /// <summary>
    /// 验证代理可用性。
    /// </summary>
    /// <param name="proxy">待验证代理。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>验证结果。</returns>
    public async ValueTask<ValidationResult> ValidateAsync(ProxyInfo proxy, CancellationToken ct = default)
    {
        MaybeCleanup();

        var sw = Stopwatch.StartNew();
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(options.Timeout);

        try
        {
            var invoker = GetOrCreateInvoker(proxy);
            using var request = new HttpRequestMessage(HttpMethod.Get, options.ValidationUrl);
            CopyDefaultHeaders(request);

            var response = await invoker.SendAsync(request, timeoutCts.Token).ConfigureAwait(false);
            sw.Stop();

            if (options.ValidationFunc is not null)
            {
                var ok = await options.ValidationFunc(response).ConfigureAwait(false);
                return ok
                    ? ValidationResult.Success(sw.Elapsed, (int)response.StatusCode)
                    : ValidationResult.Failed(ValidationResultType.InvalidResponse, "Validation function returned false.");
            }

            return response.IsSuccessStatusCode && (int)response.StatusCode == options.ExpectedStatusCode
                ? ValidationResult.Success(sw.Elapsed, (int)response.StatusCode)
                : ValidationResult.Failed(ValidationResultType.InvalidResponse, $"Unexpected status code {(int)response.StatusCode}.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            sw.Stop();
            return ValidationResult.Timeout(sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ValidationResult.Failed(ValidationResultType.Exception, ex.Message, ex);
        }
    }

    private HttpMessageInvoker GetOrCreateInvoker(ProxyInfo proxy)
    {
        var cached = _invokers.GetOrAdd(proxy.Id, _ => CreateCachedInvoker(proxy));
        cached.Touch();
        EnforceMaxCacheSize();
        return cached.Invoker;
    }

    private CachedInvoker CreateCachedInvoker(ProxyInfo proxy)
    {
        var handler = new SocketsHttpHandler
        {
            Proxy = proxy.ToWebProxy(),
            UseProxy = true,
            PooledConnectionLifetime = options.PooledConnectionLifetime,
            PooledConnectionIdleTimeout = options.PooledConnectionIdleTimeout,
            MaxConnectionsPerServer = options.MaxConnectionsPerServer
        };

        var invoker = new HttpMessageInvoker(handler, disposeHandler: true);
        var cached = new CachedInvoker(invoker);
        cached.Touch();
        return cached;
    }

    private void CopyDefaultHeaders(HttpRequestMessage request)
    {
        if (options.Client is null)
        {
            return;
        }

        foreach (var header in options.Client.DefaultRequestHeaders)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private void EnforceMaxCacheSize()
    {
        var max = options.MaxCachedProxyInvokers;
        if (max <= 0)
        {
            return;
        }

        // Best-effort: remove a few idle entries when cache grows.
        if (_invokers.Count <= max)
        {
            return;
        }

        var removed = 0;
        foreach (var kvp in _invokers)
        {
            if (_invokers.Count <= max)
            {
                break;
            }

            if (!kvp.Value.IsIdle(options.CachedProxyInvokerIdleTimeout))
            {
                continue;
            }

            if (_invokers.TryRemove(kvp.Key, out var cached))
            {
                cached.Dispose();
                removed++;
                if (removed >= 16)
                {
                    break;
                }
            }
        }
    }

    private void MaybeCleanup()
    {
        var now = DateTime.UtcNow.Ticks;
        var last = Interlocked.Read(ref _lastCleanupTicks);
        if (last != 0 && new TimeSpan(now - last) < options.CachedProxyInvokerIdleTimeout)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _lastCleanupTicks, now, last) != last)
        {
            return;
        }

        var idleTimeout = options.CachedProxyInvokerIdleTimeout;
        foreach (var kvp in _invokers)
        {
            if (!kvp.Value.IsIdle(idleTimeout))
            {
                continue;
            }

            if (_invokers.TryRemove(kvp.Key, out var cached))
            {
                cached.Dispose();
            }
        }
    }

    public void Dispose()
    {
        foreach (var kvp in _invokers)
        {
            if (_invokers.TryRemove(kvp.Key, out var cached))
            {
                cached.Dispose();
            }
        }
    }
}
