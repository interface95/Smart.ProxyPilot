using Smart.ProxyPilot.Abstractions;
using Smart.ProxyPilot.Models;

namespace Smart.ProxyPilot.Validators;

public class CompositeProxyValidator(IEnumerable<IProxyValidator> validators) : IProxyValidator
{
    private readonly IReadOnlyList<IProxyValidator> _validators = validators.ToList();

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
