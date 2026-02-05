using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Parsers;

public class LineProxyParser() : IProxyParser
{
    /// <summary>
    /// 解析按行分隔的 ip:port 内容。
    /// </summary>
    /// <param name="content">原始内容。</param>
    /// <returns>代理列表。</returns>
    public IEnumerable<ProxyInfo> Parse(string content)
    {
        foreach (var raw in content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
            {
                continue;
            }

            yield return new ProxyInfo(parts[0], port, ProxyType.Http) { Source = "Api" };
        }
    }
}
