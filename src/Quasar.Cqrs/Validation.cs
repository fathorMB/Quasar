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
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        foreach (var v in _validators)
        {
            await v.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        }
        return await next().ConfigureAwait(false);
    }
}

