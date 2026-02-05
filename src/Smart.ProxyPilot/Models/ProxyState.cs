namespace Smart.ProxyPilot.Models;

public enum ProxyState
{
    Pending,
    Validating,
    Available,
    InUse,
    Cooldown,
    Disabled,
    Expired
}
