using Microsoft.Extensions.Logging;

namespace Quasar.Cqrs;

public interface ICommandTransaction
{
    Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default);
}

public sealed class NoopCommandTransaction : ICommandTransaction
{
    public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
        => action(cancellationToken);
}

public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ICommandTransaction? _transaction;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(ICommandTransaction? transaction = null, ILogger<TransactionBehavior<TRequest, TResponse>>? logger = null)
    {
        _transaction = transaction;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TransactionBehavior<TRequest, TResponse>>.Instance;
    }

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
