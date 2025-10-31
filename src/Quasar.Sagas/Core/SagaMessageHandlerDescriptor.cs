using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Cqrs;

namespace Quasar.Sagas.Core;

internal sealed class SagaMessageHandlerDescriptor
{
    private readonly Func<IServiceProvider, IMediator, ISagaState, object, CancellationToken, Task<SagaExecutionResult>> _invoker;

    public SagaMessageHandlerDescriptor(
        Type sagaType,
        Type stateType,
        Type messageType,
        bool isStarter,
        Func<object, Guid> correlationResolver,
        Func<IServiceProvider, IMediator, ISagaState, object, CancellationToken, Task<SagaExecutionResult>> invoker)
    {
        SagaType = sagaType;
        StateType = stateType;
        MessageType = messageType;
        IsStarter = isStarter;
        _invoker = invoker;
        CorrelationResolver = correlationResolver;
    }

    public Type SagaType { get; }

    public Type StateType { get; }

    public Type MessageType { get; }

    public bool IsStarter { get; }

    public Func<object, Guid> CorrelationResolver { get; }

    public Task<SagaExecutionResult> InvokeAsync(IServiceProvider services, IMediator mediator, ISagaState state, object message, CancellationToken cancellationToken)
        => _invoker(services, mediator, state, message, cancellationToken);

    public static SagaMessageHandlerDescriptor Create<TSaga, TState, TMessage>(
        bool isStarter,
        Func<TMessage, Guid> correlationResolver)
        where TSaga : class
        where TState : class, ISagaState, new()
    {
        if (correlationResolver is null) throw new ArgumentNullException(nameof(correlationResolver));

        var correlation = new Func<object, Guid>(message =>
        {
            if (message is TMessage typed) return correlationResolver(typed);
            throw new InvalidOperationException($"Saga correlation delegate expected message of type {typeof(TMessage).Name} but received {message?.GetType().Name ?? "<null>"}.");
        });

        var servicesParam = Expression.Parameter(typeof(IServiceProvider), "services");
        var mediatorParam = Expression.Parameter(typeof(IMediator), "mediator");
        var stateParam = Expression.Parameter(typeof(ISagaState), "state");
        var messageParam = Expression.Parameter(typeof(object), "message");
        var cancellationParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var sagaType = typeof(TSaga);
        var handlerInterface = typeof(ISagaHandler<,>).MakeGenericType(typeof(TMessage), typeof(TState));
        if (!handlerInterface.IsAssignableFrom(sagaType))
        {
            throw new InvalidOperationException($"{sagaType.Name} must implement {handlerInterface.Name} to handle {typeof(TMessage).Name}.");
        }

        var getServiceCall = Expression.Call(typeof(ServiceProviderServiceExtensions), nameof(ServiceProviderServiceExtensions.GetRequiredService), new[] { sagaType }, servicesParam);
        var castSaga = Expression.Convert(getServiceCall, handlerInterface);
        var castState = Expression.Convert(stateParam, typeof(TState));
        var castMessage = Expression.Convert(messageParam, typeof(TMessage));

        var handleMethod = handlerInterface.GetMethod(nameof(ISagaHandler<TMessage, TState>.HandleAsync))
                          ?? throw new InvalidOperationException($"HandleAsync method not found on {handlerInterface.Name}.");

        var contextCtor = typeof(SagaContext<TState>).GetConstructor(new[] { typeof(TState), typeof(IServiceProvider), typeof(IMediator), typeof(CancellationToken) })
                          ?? typeof(SagaContext<TState>).GetConstructor(new[] { typeof(TState), typeof(IServiceProvider), typeof(IMediator) })
                          ?? throw new InvalidOperationException($"SagaContext<{typeof(TState).Name}> constructor missing.");

        Expression[] ctorArgs;
        if (contextCtor.GetParameters().Length == 4)
        {
            ctorArgs = new Expression[] { castState, servicesParam, mediatorParam, cancellationParam };
        }
        else
        {
            ctorArgs = new Expression[] { castState, servicesParam, mediatorParam };
        }

        var context = Expression.New(contextCtor, ctorArgs);
        var call = Expression.Call(castSaga, handleMethod, context, castMessage, cancellationParam);

        var lambda = Expression.Lambda<Func<IServiceProvider, IMediator, ISagaState, object, CancellationToken, Task<SagaExecutionResult>>>(
            call,
            servicesParam,
            mediatorParam,
            stateParam,
            messageParam,
            cancellationParam);

        return new SagaMessageHandlerDescriptor(
            sagaType,
            typeof(TState),
            typeof(TMessage),
            isStarter,
            correlation,
            lambda.Compile());
    }
}
