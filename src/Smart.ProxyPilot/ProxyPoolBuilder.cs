using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Options;
using Smart.ProxyPilot.Providers;
using Smart.ProxyPilot.Scheduling;
using Smart.ProxyPilot.Storage;
using Smart.ProxyPilot.Validators;

namespace Smart.ProxyPilot;

public class ProxyPoolBuilder()
{
    private readonly ProxyPoolOptions _options = new();
    private readonly List<IProxyProvider> _providers = [];
    private IProxyValidator? _validator;
    private IProxyScheduler? _scheduler;
    private IProxyStorage? _storage;

    public ProxyPoolBuilder Configure(Action<ProxyPoolOptions> configure)
    {
        configure(_options);
        return this;
    }

    public ProxyPoolBuilder AddProvider(IProxyProvider provider)
    {
        _providers.Add(provider);
        return this;
    }

    public ProxyPoolBuilder AddProvider<T>() where T : IProxyProvider, new()
        => AddProvider(new T());

    public ProxyPoolBuilder AddApiProvider(Uri apiUrl, IProxyParser? parser = null, HttpClient? client = null)
    {
        var options = new ApiProxyProviderOptions(apiUrl)
        {
            Parser = parser,
            Client = client
        };
        return AddProvider(new ApiProxyProvider(options));
    }

    public ProxyPoolBuilder UseValidator(IProxyValidator validator)
    {
        _validator = validator;
        return this;
    }

    public ProxyPoolBuilder UseValidator<T>() where T : IProxyValidator, new()
        => UseValidator(new T());

    public ProxyPoolBuilder UseScheduler(IProxyScheduler scheduler)
    {
        _scheduler = scheduler;
        return this;
    }

    public ProxyPoolBuilder UseScheduler<T>() where T : IProxyScheduler, new()
        => UseScheduler(new T());

    public ProxyPoolBuilder UseStorage(IProxyStorage storage)
    {
        _storage = storage;
        return this;
    }

    public ProxyPoolBuilder UseStorage<T>() where T : IProxyStorage, new()
        => UseStorage(new T());

    public IProxyPool Build()
    {
        if (_providers.Count == 0)
        {
            throw new InvalidOperationException("At least one provider is required.");
        }

        _validator ??= new HttpProxyValidator(new HttpProxyValidatorOptions(new Uri(_options.ValidationUrl))
        {
            Timeout = _options.ValidationTimeout,
            ValidationFunc = _options.ValidationFunc
        });

        _scheduler ??= new WeightedScheduler();
        _storage ??= new InMemoryProxyStorage();

        return new ProxyPool(_options, _providers, _validator, _scheduler, _storage);
    }
}
