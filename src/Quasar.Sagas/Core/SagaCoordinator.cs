using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quasar.Cqrs;
using Quasar.Sagas.Persistence;
using System.Reflection;

namespace Quasar.Sagas.Core;

internal interface ISagaCoordinator
{
    Task ProcessAsync(object message, CancellationToken cancellationToken);
}

internal sealed class SagaCoordinator : ISagaCoordinator
{
    private static readonly MethodInfo FindStateMethod = typeof(SagaCoordinator).GetMethod(nameof(FindStateAsyncCore), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo SaveStateMethod = typeof(SagaCoordinator).GetMethod(nameof(SaveStateAsyncCore), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo DeleteStateMethod = typeof(SagaCoordinator).GetMethod(nameof(DeleteStateAsyncCore), BindingFlags.NonPublic | BindingFlags.Static)!;

    private readonly IServiceProvider _provider;
    private readonly IMediator _mediator;
    private readonly ISagaRegistry _registry;
    private readonly ILogger<SagaCoordinator> _logger;

    public SagaCoordinator(
        IServiceProvider provider,
        IMediator mediator,
        ISagaRegistry registry,
        ILogger<SagaCoordinator> logger)
    {
        _provider = provider;
        _mediator = mediator;
        _registry = registry;
        _logger = logger;
    }

    public async Task ProcessAsync(object message, CancellationToken cancellationToken)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));

        var messageType = message.GetType();
        var handlers = _registry.Resolve(messageType);
        if (handlers.Count == 0)
        {
            return;
        }

        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;

        foreach (var descriptor in handlers)
        {
            await ProcessDescriptorAsync(descriptor, services, message, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessDescriptorAsync(
        SagaMessageHandlerDescriptor descriptor,
        IServiceProvider services,
        object message,
        CancellationToken cancellationToken)
    {
        Guid sagaId;
        try
        {
            sagaId = descriptor.CorrelationResolver(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga correlation failed for {SagaType} handling {MessageType}", descriptor.SagaType.Name, descriptor.MessageType.Name);
            return;
        }

        if (sagaId == Guid.Empty)
        {
            if (!descriptor.IsStarter)
            {
                _logger.LogWarning("Saga correlation resolved to empty Guid for {SagaType}. Message will be ignored.", descriptor.SagaType.Name);
                return;
            }

            sagaId = Guid.NewGuid();
        }

        var repositoryType = typeof(ISagaRepository<>).MakeGenericType(descriptor.StateType);
        var repository = services.GetService(repositoryType);
        if (repository is null)
        {
            _logger.LogError("Saga repository for {StateType} not registered. {SagaType} will be skipped.", descriptor.StateType.Name, descriptor.SagaType.Name);
            return;
        }

        var state = await InvokeFindAsync(descriptor.StateType, repository, sagaId, cancellationToken).ConfigureAwait(false);
        var isNew = state is null;

        if (isNew)
        {
            if (!descriptor.IsStarter)
            {
                _logger.LogDebug("Saga {SagaType} has no existing instance for {MessageType}; skipping.", descriptor.SagaType.Name, descriptor.MessageType.Name);
                return;
            }

            state = (ISagaState?)Activator.CreateInstance(descriptor.StateType)
                ?? throw new InvalidOperationException($"Unable to create saga state of type {descriptor.StateType.Name}.");

            state.Id = sagaId;
            state.IsCompleted = false;
            state.UpdatedUtc = DateTimeOffset.UtcNow;
        }
        else if (state.IsCompleted)
        {
            _logger.LogDebug("Saga {SagaType}:{SagaId} already completed. Message {MessageType} ignored.", descriptor.SagaType.Name, sagaId, descriptor.MessageType.Name);
            return;
        }

        try
        {
            var result = await descriptor.InvokeAsync(services, _mediator, state!, message, cancellationToken).ConfigureAwait(false);
            state!.UpdatedUtc = DateTimeOffset.UtcNow;

            if (result.IsCompleted)
            {
                _logger.LogInformation("Saga {SagaType}:{SagaId} completed after handling {MessageType}.", descriptor.SagaType.Name, sagaId, descriptor.MessageType.Name);
                state.IsCompleted = true;
                await InvokeDeleteAsync(descriptor.StateType, repository, sagaId, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await InvokeSaveAsync(descriptor.StateType, repository, state!, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga {SagaType}:{SagaId} failed while handling {MessageType}.", descriptor.SagaType.Name, sagaId, descriptor.MessageType.Name);
            throw;
        }
    }

    private static Task<ISagaState?> InvokeFindAsync(Type stateType, object repository, Guid sagaId, CancellationToken cancellationToken)
    {
        var method = FindStateMethod.MakeGenericMethod(stateType);
        return (Task<ISagaState?>)method.Invoke(null, new object?[] { repository, sagaId, cancellationToken })!;
    }

    private static Task InvokeSaveAsync(Type stateType, object repository, ISagaState state, CancellationToken cancellationToken)
    {
        var method = SaveStateMethod.MakeGenericMethod(stateType);
        return (Task)method.Invoke(null, new object?[] { repository, state, cancellationToken })!;
    }

    private static Task InvokeDeleteAsync(Type stateType, object repository, Guid sagaId, CancellationToken cancellationToken)
    {
        var method = DeleteStateMethod.MakeGenericMethod(stateType);
        return (Task)method.Invoke(null, new object?[] { repository, sagaId, cancellationToken })!;
    }

    private static async Task<ISagaState?> FindStateAsyncCore<TState>(ISagaRepository<TState> repository, Guid sagaId, CancellationToken cancellationToken)
        where TState : class, ISagaState, new()
    {
        return await repository.FindAsync(sagaId, cancellationToken).ConfigureAwait(false);
    }

    private static Task SaveStateAsyncCore<TState>(ISagaRepository<TState> repository, ISagaState state, CancellationToken cancellationToken)
        where TState : class, ISagaState, new()
    {
        if (state is not TState typedState)
        {
            throw new InvalidOperationException($"Saga state instance is not of expected type {typeof(TState).Name}.");
        }

        return repository.SaveAsync(typedState, cancellationToken);
    }

    private static Task DeleteStateAsyncCore<TState>(ISagaRepository<TState> repository, Guid sagaId, CancellationToken cancellationToken)
        where TState : class, ISagaState, new()
    {
        return repository.DeleteAsync(sagaId, cancellationToken);
    }
}
