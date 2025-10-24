namespace Quasar.Cqrs;

/// <summary>
/// Marker interface representing a command message that produces a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">Result produced by the command handler.</typeparam>
public interface ICommand<TResult> { }

/// <summary>
/// Marker interface representing a query message that produces a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">Result produced by the query handler.</typeparam>
public interface IQuery<TResult> { }

/// <summary>
/// Handles the execution of a command message.
/// </summary>
/// <typeparam name="TCommand">The command type handled by this handler.</typeparam>
/// <typeparam name="TResult">The result returned by the command.</typeparam>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Handles the command and produces a response.
    /// </summary>
    /// <param name="command">The command that should be executed.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the command is processed.</returns>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handles the execution of a query message.
/// </summary>
/// <typeparam name="TQuery">The query type handled by this handler.</typeparam>
/// <typeparam name="TResult">The result returned by the query.</typeparam>
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles the query and produces a response.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task containing the query result.</returns>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a component in the mediator pipeline that can intercept requests before and after the main handler.
/// </summary>
/// <typeparam name="TRequest">The request type being handled.</typeparam>
/// <typeparam name="TResponse">The response type produced by the request.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
{
    /// <summary>
    /// Handles the pipeline invocation.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <param name="next">Delegate that invokes the next component in the pipeline.</param>
    /// <returns>A task that yields the response.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next);
}

/// <summary>
/// Dispatches commands and queries to their respective handlers.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a command to the registered <see cref="ICommandHandler{TCommand, TResult}"/> implementation.
    /// </summary>
    /// <typeparam name="TResult">The type of response produced by the command.</typeparam>
    /// <param name="command">The command instance to execute.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a query to the registered <see cref="IQueryHandler{TQuery, TResult}"/> implementation.
    /// </summary>
    /// <typeparam name="TResult">The type of response produced by the query.</typeparam>
    /// <param name="query">The query instance to execute.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
