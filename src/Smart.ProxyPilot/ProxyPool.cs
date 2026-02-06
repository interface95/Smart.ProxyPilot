using System.Collections.Concurrent;
using System.Threading.Channels;
using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Options;
using Smart.ProxyPilot.EventSinks;

namespace Smart.ProxyPilot;

public class ProxyPool(
    ProxyPoolOptions options,
    IEnumerable<IProxyProvider> providers,
    IProxyValidator validator,
    IProxyScheduler scheduler,
    IProxyStorage storage) : IProxyPool
{
    private readonly IReadOnlyList<IProxyProvider> _providers = providers.ToList();
    private readonly IProxyValidator _validator = validator;
    private readonly IProxyScheduler _scheduler = scheduler;
    private readonly IProxyStorage _storage = storage;
    private readonly IProxyEventSink _eventSink = options.EventSink ?? new NullProxyEventSink();
    private readonly ProxyPoolState _state = new();
    private readonly Channel<ProxyInfo> _validationChannel = Channel.CreateUnbounded<ProxyInfo>();
    private readonly ConcurrentQueue<TaskCompletionSource<ProxyInfo>> _waiters = new();
    private readonly List<Task> _validationWorkers = [];
    private readonly List<CancellationTokenSource> _validationWorkerCts = [];
    private int _validationConcurrency = Math.Max(1, options.ValidationConcurrency);

    private readonly List<Task> _backgroundTasks = [];
    private CancellationTokenSource? _cts;
    private bool _started;
    private readonly object _stateLock = new();

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_started)
        {
            return;
        }

        if (_providers.Count == 0)
        {
            throw new InvalidOperationException("At least one provider is required.");
        }

        _started = true;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _validationConcurrency = Math.Max(1, options.ValidationConcurrency);

        _backgroundTasks.Add(Task.Run(() => FetchLoopAsync(_cts.Token), _cts.Token));
        EnsureValidationWorkers(_validationConcurrency);
        _backgroundTasks.Add(Task.Run(() => RevalidateLoopAsync(_cts.Token), _cts.Token));
        _backgroundTasks.Add(Task.Run(() => CooldownLoopAsync(_cts.Token), _cts.Token));

        await InitializeStateFromStorageAsync(_cts.Token).ConfigureAwait(false);

        await FetchOnceAsync(_cts.Token).ConfigureAwait(false);
    }

    private async Task InitializeStateFromStorageAsync(CancellationToken ct)
    {
        var proxies = new List<ProxyInfo>();
        foreach (var state in new[]
                 {
                     ProxyState.Pending,
                     ProxyState.Validating,
                     ProxyState.Available,
                     ProxyState.InUse,
                     ProxyState.Cooldown,
                     ProxyState.Disabled
                 })
        {
            var items = await _storage.GetByStateAsync(state, ct).ConfigureAwait(false);
            proxies.AddRange(items);
        }

        lock (_stateLock)
        {
            foreach (var proxy in proxies)
            {
                _state.AddProxy(proxy.State);
            }
        }
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (!_started || _cts is null)
        {
            return;
        }

        _cts.Cancel();
        CancelValidationWorkers();
        var allTasks = _backgroundTasks.Concat(_validationWorkers).ToList();
        try
        {
            await Task.WhenAll(allTasks).WaitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        _backgroundTasks.Clear();
        _validationWorkers.Clear();
        _validationWorkerCts.Clear();
        _cts.Dispose();
        _cts = null;
        _started = false;
    }

    public async ValueTask<ProxyInfo?> TryGetProxyAsync(CancellationToken ct = default)
    {
        RecordGetRequest();
        var available = await _storage.GetByStateAsync(ProxyState.Available, ct).ConfigureAwait(false);
        var selected = _scheduler.Select(available);
        if (selected is null)
        {
            return null;
        }

        if (options.RemoveAfterGet)
        {
            await _storage.RemoveAsync(selected.Id, ct).ConfigureAwait(false);
            RecordGetSuccess();
            RemoveState(selected.State);
            return selected;
        }

        await SetStateAsync(selected, ProxyState.InUse, ct).ConfigureAwait(false);
        RecordGetSuccess();
        return selected;
    }

    public async ValueTask<ProxyInfo> GetProxyAsync(TimeSpan? timeout = null, CancellationToken ct = default)
    {
        var selected = await TryGetProxyAsync(ct).ConfigureAwait(false);
        if (selected is not null)
        {
            return selected;
        }

        var waitTimeout = timeout ?? options.DefaultGetTimeout;
        if (waitTimeout == TimeSpan.Zero)
        {
            throw new TimeoutException("Timeout waiting for proxy.");
        }

        var tcs = new TaskCompletionSource<ProxyInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        _waiters.Enqueue(tcs);
        IncrementWaiting();

        using var timeoutCts = waitTimeout == Timeout.InfiniteTimeSpan
            ? null
            : new CancellationTokenSource(waitTimeout);
        using var timeoutReg = timeoutCts?.Token.Register(() => tcs.TrySetException(new TimeoutException("Timeout waiting for proxy.")));
        using var cancelReg = ct.Register(() => tcs.TrySetCanceled(ct));

        try
        {
            var proxy = await tcs.Task.ConfigureAwait(false);
            return proxy;
        }
        finally
        {
            DecrementWaiting();
        }
    }

    public async ValueTask<ProxyInfo?> TryGetProxyOrDefaultAsync(TimeSpan? timeout = null, CancellationToken ct = default)
    {
        try
        {
            return await GetProxyAsync(timeout, ct).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            // Convert timeout to a null result for convenience.
            return null;
        }
    }

    public void ReportSuccess(ProxyInfo proxy, TimeSpan? responseTime = null)
    {
        _scheduler.OnProxyUsed(proxy, true, responseTime);
        lock (proxy)
        {
            proxy.Statistics.RecordUse(true, responseTime, ValidationResultType.Success);
        }

        if (options.RemoveAfterGet)
        {
            _storage.RemoveAsync(proxy.Id).GetAwaiter().GetResult();
            RemoveState(proxy.State);
            return;
        }

        SetStateAsync(proxy, ProxyState.Available).GetAwaiter().GetResult();
        if (TryAssignWaiter(proxy))
        {
            SetStateAsync(proxy, ProxyState.InUse).GetAwaiter().GetResult();
            RecordGetSuccess();
        }
    }

    public void ReportFailure(ProxyInfo proxy, string? reason = null)
    {
        _scheduler.OnProxyUsed(proxy, false, null);
        lock (proxy)
        {
            proxy.Statistics.RecordUse(false, null, ValidationResultType.Exception);
        }

        if (options.RemoveAfterGet)
        {
            _storage.RemoveAsync(proxy.Id).GetAwaiter().GetResult();
            RemoveState(proxy.State);
            return;
        }

        var nextState = proxy.Statistics.ConsecutiveFailCount >= options.MaxConsecutiveFailCount
            ? ProxyState.Disabled
            : ProxyState.Cooldown;
        SetStateAsync(proxy, nextState).GetAwaiter().GetResult();
    }

    public IProxyPoolState CurrentState => _state;

    /// <summary>
    /// Current validation worker concurrency.
    /// </summary>
    public int CurrentValidationConcurrency => Volatile.Read(ref _validationConcurrency);

    /// <summary>
    /// Update validation worker count at runtime.
    /// </summary>
    public void UpdateValidationConcurrency(int newConcurrency)
    {
        if (newConcurrency < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(newConcurrency), "Concurrency must be at least 1.");
        }

        EnsureValidationWorkers(newConcurrency);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }

    private async Task FetchLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await FetchOnceAsync(ct).ConfigureAwait(false);
            await Task.Delay(options.FetchInterval, ct).ConfigureAwait(false);
        }
    }

    private async Task FetchOnceAsync(CancellationToken ct)
    {
        if (GetTotalCount() >= options.MaxPoolSize)
        {
            return;
        }

        var need = Math.Min(options.FetchBatchSize, options.MaxPoolSize - GetTotalCount());
        if (need <= 0)
        {
            return;
        }

        foreach (var provider in _providers)
        {
            var fetched = await provider.FetchAsync(need, ct).ConfigureAwait(false);
            foreach (var proxy in fetched)
            {
                if (await _storage.GetByIdAsync(proxy.Id, ct).ConfigureAwait(false) is not null)
                {
                    continue;
                }

                proxy.State = ProxyState.Pending;
                await _storage.AddAsync(proxy, ct).ConfigureAwait(false);
                AddState(proxy.State);
                if (ShouldEnqueueFetchedProxyForValidation())
                {
                    await _validationChannel.Writer.WriteAsync(proxy, ct).ConfigureAwait(false);
                }
            }
        }
    }

    private bool ShouldEnqueueFetchedProxyForValidation()
    {
        if (options.MaxAvailableCountForValidation is null || options.MaxAvailableCountForValidation <= 0)
        {
            return true;
        }

        lock (_stateLock)
        {
            return _state.AvailableCount < options.MaxAvailableCountForValidation.Value;
        }
    }

    private async Task ValidateProxyAsync(ProxyInfo proxy, CancellationToken ct)
    {
        if (IsExpired(proxy))
        {
            await SetStateAsync(proxy, ProxyState.Expired, ct).ConfigureAwait(false);
            await _storage.RemoveAsync(proxy.Id, ct).ConfigureAwait(false);
            RemoveState(proxy.State);
            return;
        }

        await SetStateAsync(proxy, ProxyState.Validating, ct).ConfigureAwait(false);
        var result = await _validator.ValidateAsync(proxy, ct).ConfigureAwait(false);
            lock (proxy)
            {
                proxy.Statistics.RecordValidation(result);
            }

            _eventSink.OnProxyValidated(proxy, result);
            RecordValidation(result);

            if (result.IsSuccess)
            {
                await SetStateAsync(proxy, ProxyState.Available, ct).ConfigureAwait(false);
                if (TryAssignWaiter(proxy))
                {
                    await SetStateAsync(proxy, ProxyState.InUse, ct).ConfigureAwait(false);
                    RecordGetSuccess();
                }
            }
        else
        {
            var nextState = proxy.Statistics.ConsecutiveFailCount >= options.MaxConsecutiveFailCount
                ? ProxyState.Disabled
                : ProxyState.Cooldown;
            await SetStateAsync(proxy, nextState, ct).ConfigureAwait(false);
        }
    }

    private void EnsureValidationWorkers(int desired)
    {
        lock (_validationWorkers)
        {
            var current = _validationWorkers.Count;
            if (desired == current)
            {
                Volatile.Write(ref _validationConcurrency, desired);
                return;
            }

            if (desired > current)
            {
                var toAdd = desired - current;
                for (var i = 0; i < toAdd; i++)
                {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts?.Token ?? CancellationToken.None);
                    _validationWorkerCts.Add(cts);
                    _validationWorkers.Add(Task.Run(() => ValidationWorkerAsync(cts.Token), cts.Token));
                }
            }
            else
            {
                var toRemove = current - desired;
                for (var i = 0; i < toRemove; i++)
                {
                    var index = _validationWorkerCts.Count - 1;
                    _validationWorkerCts[index].Cancel();
                    _validationWorkerCts.RemoveAt(index);
                    _validationWorkers.RemoveAt(index);
                }
            }

            Volatile.Write(ref _validationConcurrency, desired);
        }
    }

    private void CancelValidationWorkers()
    {
        lock (_validationWorkers)
        {
            foreach (var cts in _validationWorkerCts)
            {
                cts.Cancel();
                cts.Dispose();
            }

            _validationWorkerCts.Clear();
        }
    }

    private async Task ValidationWorkerAsync(CancellationToken ct)
    {
        var reader = _validationChannel.Reader;
        while (!ct.IsCancellationRequested && await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var proxy))
            {
                await ValidateProxyAsync(proxy, ct).ConfigureAwait(false);
                if (ct.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private async Task RevalidateLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var available = await _storage.GetByStateAsync(ProxyState.Available, ct).ConfigureAwait(false);
            foreach (var proxy in available)
            {
                if (proxy.Statistics.LastValidatedAt is null ||
                    DateTime.UtcNow - proxy.Statistics.LastValidatedAt.Value >= options.ValidationInterval)
                {
                    await SetStateAsync(proxy, ProxyState.Pending, ct).ConfigureAwait(false);
                    await _validationChannel.Writer.WriteAsync(proxy, ct).ConfigureAwait(false);
                }
            }

            await Task.Delay(options.ValidationInterval, ct).ConfigureAwait(false);
        }
    }

    private async Task CooldownLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var cooldown = await _storage.GetByStateAsync(ProxyState.Cooldown, ct).ConfigureAwait(false);
            foreach (var proxy in cooldown)
            {
                if (proxy.Statistics.LastFailAt is null)
                {
                    continue;
                }

                if (DateTime.UtcNow - proxy.Statistics.LastFailAt.Value >= options.CooldownDuration)
                {
                    await SetStateAsync(proxy, ProxyState.Pending, ct).ConfigureAwait(false);
                    await _validationChannel.Writer.WriteAsync(proxy, ct).ConfigureAwait(false);
                }
            }

            await Task.Delay(options.CooldownDuration, ct).ConfigureAwait(false);
        }
    }

    private async Task SetStateAsync(ProxyInfo proxy, ProxyState state, CancellationToken ct = default)
    {
        ProxyState oldState;
        lock (proxy)
        {
            oldState = proxy.State;
            if (oldState == state)
            {
                return;
            }

            proxy.State = state;
        }

        await _storage.UpdateAsync(proxy, ct).ConfigureAwait(false);
        UpdateStateChange(oldState, state);
        _eventSink.OnProxyStateChanged(proxy, oldState, state);
        _eventSink.OnPoolStateChanged(_state);
    }

    private bool TryAssignWaiter(ProxyInfo proxy)
    {
        while (_waiters.TryDequeue(out var waiter))
        {
            if (waiter.Task.IsCompleted)
            {
                continue;
            }

            if (waiter.TrySetResult(proxy))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsExpired(ProxyInfo proxy)
        => DateTime.UtcNow - proxy.Statistics.CreatedAt >= options.ProxyExpireTime;

    private int GetTotalCount()
    {
        lock (_stateLock)
        {
            return _state.TotalCount;
        }
    }

    private void AddState(ProxyState state)
    {
        lock (_stateLock)
        {
            _state.AddProxy(state);
        }
    }

    private void RemoveState(ProxyState state)
    {
        lock (_stateLock)
        {
            _state.RemoveProxy(state);
        }
    }

    private void UpdateStateChange(ProxyState oldState, ProxyState newState)
    {
        lock (_stateLock)
        {
            _state.ChangeState(oldState, newState);
        }
    }

    private void RecordValidation(ValidationResult result)
    {
        lock (_stateLock)
        {
            _state.RecordValidation(result);
        }
    }

    private void RecordGetRequest()
    {
        lock (_stateLock)
        {
            _state.RecordGetRequest();
        }
    }

    private void RecordGetSuccess()
    {
        lock (_stateLock)
        {
            _state.RecordGetSuccess();
        }
    }

    private void IncrementWaiting()
    {
        lock (_stateLock)
        {
            _state.IncrementWaiting();
        }
    }

    private void DecrementWaiting()
    {
        lock (_stateLock)
        {
            _state.DecrementWaiting();
        }
    }
}
