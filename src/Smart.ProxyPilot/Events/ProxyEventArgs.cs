using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Events;

public class ProxyEventArgs(ProxyInfo proxy) : EventArgs
{
    public ProxyInfo Proxy { get; } = proxy;
}
