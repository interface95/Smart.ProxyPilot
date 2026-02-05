using System.Diagnostics;
using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;
using Smart.ProxyPilot.Options;

namespace Smart.ProxyPilot.Validators;

public class HttpProxyValidator(HttpProxyValidatorOptions options) : IProxyValidator
{
    /// <summary>
    /// 验证代理可用性。
    /// </summary>
    /// <param name="proxy">待验证代理。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>验证结果。</returns>
    public async ValueTask<ValidationResult> ValidateAsync(ProxyInfo proxy, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        using var handler = new HttpClientHandler
        {
            Proxy = proxy.ToWebProxy(),
            UseProxy = true
        };

        using var client = new HttpClient(handler);
        if (options.Client is not null)
        {
            foreach (var header in options.Client.DefaultRequestHeaders)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(options.Timeout);

        try
        {
            var response = await client.GetAsync(options.ValidationUrl, timeoutCts.Token).ConfigureAwait(false);
            sw.Stop();

            if (options.ValidationFunc is not null)
            {
                var ok = await options.ValidationFunc(response).ConfigureAwait(false);
                return ok
                    ? ValidationResult.Success(sw.Elapsed, (int)response.StatusCode)
                    : ValidationResult.Failed(ValidationResultType.InvalidResponse, "Validation function returned false.");
            }

            return response.IsSuccessStatusCode && (int)response.StatusCode == options.ExpectedStatusCode
                ? ValidationResult.Success(sw.Elapsed, (int)response.StatusCode)
                : ValidationResult.Failed(ValidationResultType.InvalidResponse, $"Unexpected status code {(int)response.StatusCode}.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            sw.Stop();
            return ValidationResult.Timeout(sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ValidationResult.Failed(ValidationResultType.Exception, ex.Message, ex);
        }
    }
}
