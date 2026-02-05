namespace Smart.ProxyPilot.Models;

public enum ProxyState
{
    /// <summary>
    /// 待验证。
    /// </summary>
    Pending,
    /// <summary>
    /// 验证中。
    /// </summary>
    Validating,
    /// <summary>
    /// 可用。
    /// </summary>
    Available,
    /// <summary>
    /// 使用中。
    /// </summary>
    InUse,
    /// <summary>
    /// 冷却中。
    /// </summary>
    Cooldown,
    /// <summary>
    /// 已禁用。
    /// </summary>
    Disabled,
    /// <summary>
    /// 已过期。
    /// </summary>
    Expired
}
