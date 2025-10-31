namespace Quasar.Sagas;

/// <summary>
/// Handles saga transitions triggered by a message.
/// </summary>
/// <typeparam name="TMessage">Message type.</typeparam>
/// <typeparam name="TState">Saga state type.</typeparam>
public interface ISagaHandler<TMessage, TState>
    where TState : class, ISagaState, new()
{
    /// <summary>
    /// Applies the saga transition for the provided <paramref name="message"/>.
    /// </summary>
    Task<SagaExecutionResult> HandleAsync(SagaContext<TState> context, TMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Marker interface for saga handlers that begin a new saga instance when <typeparamref name="TMessage"/> is received.
/// </summary>
public interface ISagaStartedBy<TMessage, TState> : ISagaHandler<TMessage, TState>
    where TState : class, ISagaState, new()
{
}

/// <summary>
/// Marker interface for saga handlers that operate on existing saga instances.
/// </summary>
public interface ISagaHandles<TMessage, TState> : ISagaHandler<TMessage, TState>
    where TState : class, ISagaState, new()
{
}
