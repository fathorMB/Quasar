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

    public TransactionBehavior(ICommandTransaction? transaction = null)
    {
        _transaction = transaction;
    }

    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        // Only wrap commands; for queries, just continue
        var isCommand = request is ICommand<TResponse>;
        if (!isCommand || _transaction is null)
        {
            return next();
        }
        return _transaction.ExecuteAsync<TResponse>(_ => next(), cancellationToken);
    }
}

