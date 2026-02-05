using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Abstractions;

public interface IProxyParser
{
    /// <summary>
    /// 解析原始内容为代理列表。
    /// </summary>
    /// <param name="content">原始内容。</param>
    /// <returns>代理列表。</returns>
    IEnumerable<ProxyInfo> Parse(string content);
}
