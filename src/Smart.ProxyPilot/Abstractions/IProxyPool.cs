using Smart.ProxyPilot.Events;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyPool : IAsyncDisposable
{
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);

    ValueTask<ProxyInfo?> TryGetProxyAsync(CancellationToken ct = default);
    ValueTask<ProxyInfo> GetProxyAsync(TimeSpan? timeout = null, CancellationToken ct = default);
    /// <summary>
    /// Wait for a proxy up to the timeout and return null on timeout.
    /// </summary>
    ValueTask<ProxyInfo?> TryGetProxyOrDefaultAsync(TimeSpan? timeout = null, CancellationToken ct = default);

    int CurrentValidationConcurrency { get; }
    void UpdateValidationConcurrency(int newConcurrency);

    void ReportSuccess(ProxyInfo proxy, TimeSpan? responseTime = null);
    void ReportFailure(ProxyInfo proxy, string? reason = null);

    ProxyPoolSnapshot GetSnapshot();

    event EventHandler<ProxyValidatedEventArgs>? ProxyValidated;
    event EventHandler<ProxyStateChangedEventArgs>? ProxyStateChanged;
    event EventHandler<PoolStateChangedEventArgs>? PoolStateChanged;
}
