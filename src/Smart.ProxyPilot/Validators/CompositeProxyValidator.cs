using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Validators;

public class CompositeProxyValidator(IEnumerable<IProxyValidator> validators) : IProxyValidator
{
    private readonly IReadOnlyList<IProxyValidator> _validators = validators.ToList();

    /// <summary>
    /// 依次执行验证器，任一失败则返回失败。
    /// </summary>
    /// <param name="proxy">待验证代理。</param>
    /// <param name="ct">取消令牌。</param>
    public async ValueTask<ValidationResult> ValidateAsync(ProxyInfo proxy, CancellationToken ct = default)
    {
        if (_validators.Count == 0)
        {
            return ValidationResult.Failed(ValidationResultType.Exception, "No validators configured.");
        }

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(proxy, ct).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return result;
            }
        }

        return ValidationResult.Success(TimeSpan.Zero, 200);
    }
}
