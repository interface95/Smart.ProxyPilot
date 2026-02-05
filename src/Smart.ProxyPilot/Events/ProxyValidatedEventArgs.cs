using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Events;

public class ProxyValidatedEventArgs(ProxyInfo proxy, ValidationResult result) : ProxyEventArgs(proxy)
{
    public ValidationResult Result { get; } = result;
}
