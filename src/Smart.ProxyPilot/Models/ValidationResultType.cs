namespace Smart.ProxyPilot.Models;

public enum ValidationResultType
{
    /// <summary>
    /// 成功。
    /// </summary>
    Success,
    /// <summary>
    /// 超时。
    /// </summary>
    Timeout,
    /// <summary>
    /// 连接失败。
    /// </summary>
    ConnectionFailed,
    /// <summary>
    /// 认证失败。
    /// </summary>
    AuthenticationFailed,
    /// <summary>
    /// 响应无效。
    /// </summary>
    InvalidResponse,
    /// <summary>
    /// 其他异常。
    /// </summary>
    Exception
}
