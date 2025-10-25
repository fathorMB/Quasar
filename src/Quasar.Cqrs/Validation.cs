using Microsoft.Extensions.Logging;
using Quasar.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Quasar.Cqrs;

/// <summary>
/// Defines validation logic that can be applied to requests prior to handler execution.
/// </summary>
/// <typeparam name="T">The type to validate.</typeparam>
public interface IValidator<in T>
{
    /// <summary>
    /// Validates the specified <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <param name="cancellationToken">Token used to cancel the validation work.</param>
    /// <returns>A list of validation errors; returns an empty list if validation is successful.</returns>
    Task<List<Error>> ValidateAsync(T instance, CancellationToken cancellationToken = default);
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

        var errorTasks = _validators.Select(v =>
        {
            _logger.LogTrace("Running validator {Validator} for {RequestType}", v.GetType().Name, typeof(TRequest).Name);
            return v.ValidateAsync(request, cancellationToken);
        });

        var errors = (await Task.WhenAll(errorTasks).ConfigureAwait(false))
            .SelectMany(e => e)
            .ToList();

        if (errors.Count == 0)
        {
            _logger.LogDebug("Validation completed for {RequestType}", typeof(TRequest).Name);
            return await next().ConfigureAwait(false);
        }

        _logger.LogWarning("Validation failed for {RequestType} with {ErrorCount} errors", typeof(TRequest).Name, errors.Count);
        return (TResponse)CreateFailureResult(errors);
    }

    private static object CreateFailureResult(List<Error> errors)
    {
        var error = new Error("Validation.Failed", AggregateMessages(errors));

        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(resultType)
                .GetMethod(nameof(Result<object>.Failure), BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException("Could not find the 'Failure' method on the Result type.");

            return failureMethod.Invoke(null, new object[] { error })!;
        }

        if (responseType == typeof(Result))
        {
            return Result.Failure(error);
        }

        throw new InvalidOperationException(
            $"ValidationBehavior is configured for request {typeof(TRequest).Name} but the response {responseType.Name} is not a Result or Result<T> type.");
    }

    private static string AggregateMessages(List<Error> errors)
    {
        var builder = new StringBuilder();
        foreach (var error in errors)
        {
            builder.AppendLine(error.Message);
        }

        return builder.ToString();
    }
}
