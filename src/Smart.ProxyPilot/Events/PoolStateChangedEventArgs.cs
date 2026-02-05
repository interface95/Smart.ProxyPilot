using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Events;

public class PoolStateChangedEventArgs(ProxyPoolSnapshot snapshot) : EventArgs
{
    public ProxyPoolSnapshot Snapshot { get; } = snapshot;
}
