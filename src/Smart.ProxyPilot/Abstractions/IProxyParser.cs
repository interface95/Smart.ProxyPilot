using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyParser
{
    IEnumerable<ProxyInfo> Parse(string content);
}
