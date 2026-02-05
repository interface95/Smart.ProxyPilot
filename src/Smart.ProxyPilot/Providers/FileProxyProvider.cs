using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Options;

namespace Smart.ProxyPilot.Providers;

public class FileProxyProvider(FileProxyProviderOptions options) : IProxyProvider
{
    public string Name => "File";

    public async ValueTask<IEnumerable<ProxyInfo>> FetchAsync(int count, CancellationToken ct = default)
    {
        var content = await File.ReadAllTextAsync(options.FilePath, ct).ConfigureAwait(false);
        var proxies = options.ParseFunc is not null
            ? options.ParseFunc(content)
            : ParseDefault(content);

        return count > 0 ? proxies.Take(count).ToList() : proxies.ToList();
    }

    private static IEnumerable<ProxyInfo> ParseDefault(string content)
    {
        foreach (var raw in content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            if (TryParseUri(line, out var proxy))
            {
                yield return proxy;
                continue;
            }

            var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
            {
                continue;
            }

            yield return new ProxyInfo(parts[0], port, ProxyType.Http) { Source = "File" };
        }
    }

    private static bool TryParseUri(string line, out ProxyInfo proxy)
    {
        proxy = null!;
        if (!Uri.TryCreate(line, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var type = uri.Scheme.ToLowerInvariant() switch
        {
            "http" => ProxyType.Http,
            "https" => ProxyType.Https,
            "socks4" => ProxyType.Socks4,
            "socks5" => ProxyType.Socks5,
            _ => ProxyType.Http
        };

        proxy = new ProxyInfo(uri.Host, uri.Port, type)
        {
            Username = string.IsNullOrWhiteSpace(uri.UserInfo) ? null : uri.UserInfo.Split(':')[0],
            Password = string.IsNullOrWhiteSpace(uri.UserInfo) ? null : uri.UserInfo.Split(':').Length > 1 ? uri.UserInfo.Split(':')[1] : null,
            Source = "File"
        };
        return true;
    }
}
