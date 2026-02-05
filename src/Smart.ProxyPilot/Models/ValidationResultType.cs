namespace Smart.ProxyPilot.Models;

public enum ValidationResultType
{
    Success,
    Timeout,
    ConnectionFailed,
    AuthenticationFailed,
    InvalidResponse,
    Exception
}
