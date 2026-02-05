namespace Smart.ProxyPilot.Options;

public class HttpProxyValidatorOptions(Uri validationUrl)
{
    public Uri ValidationUrl { get; } = validationUrl;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    public Func<HttpResponseMessage, ValueTask<bool>>? ValidationFunc { get; set; }
    public int ExpectedStatusCode { get; set; } = 200;
    public HttpClient? Client { get; set; }
}
