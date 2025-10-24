using Microsoft.Extensions.Logging;
using System.Linq;

namespace Quasar.Cqrs;

/// <summary>
/// Defines validation logic that can be applied to requests prior to handler execution.
/// </summary>
/// <typeparam name="T">The type to validate.</typeparam>
public interface IValidator<in T>
{
    /// <summary>
    /// Validates the specified <paramref name="instance"/> and throws when validation fails.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <param name="cancellationToken">Token used to cancel the validation work.</param>
    Task ValidateAsync(T instance, CancellationToken cancellationToken = default);
}

/// <summary>
/// Exception thrown when a validation rule fails.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with the provided message.
    /// </summary>
    public ValidationException(string message) : base(message) { }
}

/// <summary>
/// Pipeline behavior that executes all registered validators before continuing to the next component.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    /// <inheritdoc />
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
