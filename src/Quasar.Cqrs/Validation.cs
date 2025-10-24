using Microsoft.Extensions.Logging;
using System.Linq;

namespace Quasar.Cqrs;

public interface IValidator<in T>
{
    Task ValidateAsync(T instance, CancellationToken cancellationToken = default);
}

public sealed class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        if (!_validators.Any())
        {
            _logger.LogTrace("No validators registered for {RequestType}", typeof(TRequest).Name);
            return await next().ConfigureAwait(false);
        }

        foreach (var v in _validators)
        {
            _logger.LogTrace("Running validator {Validator} for {RequestType}", v.GetType().Name, typeof(TRequest).Name);
            await v.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        }
        _logger.LogDebug("Validation completed for {RequestType}", typeof(TRequest).Name);
        return await next().ConfigureAwait(false);
    }
}
