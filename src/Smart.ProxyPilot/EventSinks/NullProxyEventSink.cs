using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.EventSinks;

public class NullProxyEventSink() : IProxyEventSink
{
    public void OnProxyValidated(ProxyInfo proxy, ValidationResult result)
    {
    }

    public void OnProxyStateChanged(ProxyInfo proxy, ProxyState oldState, ProxyState newState)
    {
    }

    public void OnPoolStateChanged(ProxyPoolSnapshot snapshot)
    {
    }
}
