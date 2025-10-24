using System.Reflection;

namespace Quasar.Cqrs;

public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _provider;

    public Mediator(IServiceProvider provider) => _provider = provider;

    public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        => InvokePipeline<TResult>(command!, isCommand: true, cancellationToken);

    public Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        => InvokePipeline<TResult>(query!, isCommand: false, cancellationToken);

    private async Task<TResult> InvokePipeline<TResult>(object request, bool isCommand, CancellationToken ct)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();

        var handlerType = isCommand
            ? typeof(ICommandHandler<,>).MakeGenericType(requestType, typeof(TResult))
            : typeof(IQueryHandler<,>).MakeGenericType(requestType, typeof(TResult));

        var handler = _provider.GetService(handlerType)
                     ?? throw new InvalidOperationException($"Handler not registered for {requestType.Name} -> {typeof(TResult).Name}");

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
            next = () => (Task<TResult>)handle.Invoke(behavior, new object?[] { request, ct, currentNext })!;
        }

        return await next().ConfigureAwait(false);
    }
}
