namespace Smart.ProxyPilot.Options;

public class HttpProxyValidatorOptions(Uri validationUrl)
{
    /// <summary>
    /// 验证地址。
    /// </summary>
    public Uri ValidationUrl { get; } = validationUrl;
    /// <summary>
    /// 验证超时。
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    /// <summary>
    /// 自定义响应校验函数。
    /// </summary>
    public Func<HttpResponseMessage, ValueTask<bool>>? ValidationFunc { get; set; }
    /// <summary>
    /// 期望状态码。
    /// </summary>
    public int ExpectedStatusCode { get; set; } = 200;
    /// <summary>
    /// 自定义 HttpClient。
    /// </summary>
    public HttpClient? Client { get; set; }
}
