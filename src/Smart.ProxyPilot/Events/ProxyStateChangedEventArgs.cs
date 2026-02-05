using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Events;

public class ProxyStateChangedEventArgs(ProxyInfo proxy, ProxyState oldState, ProxyState newState) : ProxyEventArgs(proxy)
{
    public ProxyState OldState { get; } = oldState;
    public ProxyState NewState { get; } = newState;
}
