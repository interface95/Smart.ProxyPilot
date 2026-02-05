using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Options;

public class FileProxyProviderOptions(string filePath)
{
    public string FilePath { get; } = filePath;
    public Func<string, IEnumerable<ProxyInfo>>? ParseFunc { get; set; }
}
