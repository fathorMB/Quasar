using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Quasar.Cqrs;

/// <summary>
/// Default mediator implementation that resolves handlers and executes behaviors from the dependency injection container.
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<Mediator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="provider">Service provider used to resolve handlers and behaviors.</param>
    /// <param name="logger">Logger instance used for diagnostic output.</param>
    public Mediator(IServiceProvider provider, ILogger<Mediator> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Dispatching command {CommandType}", command?.GetType().Name ?? typeof(TResult).Name);
        return InvokePipeline<TResult>(command!, isCommand: true, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Dispatching query {QueryType}", query?.GetType().Name ?? typeof(TResult).Name);
        return InvokePipeline<TResult>(query!, isCommand: false, cancellationToken);
    }

    /// <summary>
    /// Executes the handler pipeline for the provided <paramref name="request"/>.
    /// </summary>
    private async Task<TResult> InvokePipeline<TResult>(object request, bool isCommand, CancellationToken ct)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();

        _logger.LogTrace("Resolving handler for {RequestType}", requestType.Name);

        var handlerType = isCommand
            ? typeof(ICommandHandler<,>).MakeGenericType(requestType, typeof(TResult))
            : typeof(IQueryHandler<,>).MakeGenericType(requestType, typeof(TResult));

        var handler = _provider.GetService(handlerType);
        if (handler is null)
        {
            _logger.LogError("Handler not registered for {RequestType} -> {ResponseType}", requestType.Name, typeof(TResult).Name);
            throw new InvalidOperationException($"Handler not registered for {requestType.Name} -> {typeof(TResult).Name}");
        }

        var behaviorsType = typeof(IEnumerable<>).MakeGenericType(typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResult)));
        var behaviors = (IEnumerable<object>?)_provider.GetService(behaviorsType) ?? Array.Empty<object>();

        var handleMethod = handlerType.GetMethod("Handle")
                          ?? throw new InvalidOperationException($"Handler {handlerType.Name} does not implement Handle method");

        Task<TResult> HandlerInvoker()
            => (Task<TResult>)handleMethod.Invoke(handler, new object?[] { request, ct })!;

        Func<Task<TResult>> next = HandlerInvoker;
        foreach (var behavior in behaviors.Reverse())
        {
            var behaviorType = behavior.GetType();
            var handle = behaviorType.GetMethod("Handle")
                         ?? throw new InvalidOperationException($"Behavior {behaviorType.Name} does not implement Handle method");
            var currentNext = next;
            next = () =>
            {
                _logger.LogTrace("Executing behavior {Behavior} for {RequestType}", behaviorType.Name, requestType.Name);
                return (Task<TResult>)handle.Invoke(behavior, new object?[] { request, ct, currentNext })!;
            };
        }

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await next().ConfigureAwait(false);
            sw.Stop();
            _logger.LogDebug("Completed {RequestType} in {Elapsed}ms", requestType.Name, sw.Elapsed.TotalMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Error executing {RequestType} after {Elapsed}ms", requestType.Name, sw.Elapsed.TotalMilliseconds);
            throw;
        }
    }
}
