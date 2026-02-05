using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Options;

public class FileProxyProviderOptions(string filePath)
{
    /// <summary>
    /// 文件路径。
    /// </summary>
    public string FilePath { get; } = filePath;
    /// <summary>
    /// 自定义解析函数。
    /// </summary>
    public Func<string, IEnumerable<ProxyInfo>>? ParseFunc { get; set; }
}
