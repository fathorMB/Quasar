using Quasar.EventSourcing.Abstractions;

namespace Quasar.Projections.Abstractions;

/// <summary>
/// Represents a projection that operates in real-time, updating read models synchronously
/// with event commits and broadcasting changes immediately to subscribers.
/// </summary>
/// <typeparam name="TEvent">The domain event type handled by this projection.</typeparam>
public interface ILiveProjection<in TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handles an event and broadcasts the update in real-time.
    /// </summary>
    /// <param name="event">The event that occurred.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <remarks>
    /// This method is called synchronously within the event store transaction,
    /// ensuring consistency between event persistence and read model updates.
    /// </remarks>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Stores read model data with real-time update capabilities.
/// </summary>
public interface ILiveReadModelStore
{
    /// <summary>
    /// Updates or inserts a read model document.
    /// </summary>
    Task UpsertAsync<TReadModel>(string key, TReadModel model, CancellationToken cancellationToken = default)
        where TReadModel : class;

    /// <summary>
    /// Retrieves a read model document by key.
    /// </summary>
    Task<TReadModel?> GetAsync<TReadModel>(string key, CancellationToken cancellationToken = default)
        where TReadModel : class;

    /// <summary>
    /// Deletes a read model document.
    /// </summary>
    Task DeleteAsync<TReadModel>(string key, CancellationToken cancellationToken = default)
        where TReadModel : class;

    /// <summary>
    /// Retrieves all read model documents of a given type.
    /// </summary>
    Task<IReadOnlyList<TReadModel>> GetAllAsync<TReadModel>(CancellationToken cancellationToken = default)
        where TReadModel : class;
}

/// <summary>
/// Broadcasts read model changes to real-time subscribers.
/// </summary>
public interface ILiveReadModelNotifier
{
    /// <summary>
    /// Notifies subscribers of a read model being upserted.
    /// </summary>
    /// <typeparam name="TReadModel">The read model type.</typeparam>
    /// <param name="key">Unique identifier of the model.</param>
    /// <param name="model">The updated model data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task NotifyUpsertAsync<TReadModel>(string key, TReadModel model, CancellationToken cancellationToken = default)
        where TReadModel : class;

    /// <summary>
    /// Notifies subscribers of a read model being deleted.
    /// </summary>
    /// <typeparam name="TReadModel">The read model type.</typeparam>
    /// <param name="key">Unique identifier of the model.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task NotifyDeleteAsync<TReadModel>(string key, CancellationToken cancellationToken = default)
        where TReadModel : class;
}

/// <summary>
/// Pipeline behavior for live projection that manages real-time synchronization.
/// </summary>
public interface ILiveProjectionPipeline
{
    /// <summary>
    /// Registers a live projection handler for a specific event type.
    /// </summary>
    void RegisterProjection(Type eventType, Type projectionType);

    /// <summary>
    /// Dispatches an event to all registered live projections.
    /// </summary>
    Task DispatchAsync(EventEnvelope envelope, CancellationToken cancellationToken = default);
}
