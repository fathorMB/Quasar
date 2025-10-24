using Microsoft.Extensions.Logging;

namespace Quasar.Cqrs;

/// <summary>
/// Represents an ambient transaction surrounding command execution.
/// </summary>
public interface ICommandTransaction
{
    /// <summary>
    /// Executes the supplied <paramref name="action"/> within the transaction boundary.
    /// </summary>
    /// <typeparam name="TResult">The type of response produced by the command.</typeparam>
    /// <param name="action">Delegate that performs the work inside the transaction.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default);
}

/// <summary>
/// No-op transaction implementation used when no transactional resource is required.
/// </summary>
public sealed class NoopCommandTransaction : ICommandTransaction
{
    /// <inheritdoc />
    public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
        => action(cancellationToken);
}

/// <summary>
/// Mediator behavior that surrounds command execution with an <see cref="ICommandTransaction"/>.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ICommandTransaction? _transaction;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    public TransactionBehavior(ICommandTransaction? transaction = null, ILogger<TransactionBehavior<TRequest, TResponse>>? logger = null)
    {
        _transaction = transaction;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TransactionBehavior<TRequest, TResponse>>.Instance;
    }

    /// <inheritdoc />
    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        // Only wrap commands; for queries, just continue
        var isCommand = request is ICommand<TResponse>;
        if (!isCommand || _transaction is null)
        {
            _logger.LogTrace("No transaction wrapping for {RequestType}", typeof(TRequest).Name);
            return next();
        }
        _logger.LogTrace("Executing {RequestType} inside transaction", typeof(TRequest).Name);
        return _transaction.ExecuteAsync<TResponse>(async _ =>
        {
            var result = await next().ConfigureAwait(false);
            _logger.LogDebug("Transaction completed for {RequestType}", typeof(TRequest).Name);
            return result;
        }, cancellationToken);
    }
}
