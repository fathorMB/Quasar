using Quasar.Domain;

namespace Quasar.EventSourcing.Abstractions;

/// <summary>
/// Marker interface for events persisted within the event store. Extends <see cref="Quasar.Domain.IDomainEvent"/>.
/// </summary>
public interface IEvent : Quasar.Domain.IDomainEvent { }

/// <summary>
/// Represents a persisted event including metadata used by the event store infrastructure.
/// </summary>
/// <param name="StreamId">The identifier of the stream the event belongs to.</param>
/// <param name="Version">The stream version associated with this event.</param>
/// <param name="CreatedUtc">Timestamp captured when the event was persisted.</param>
/// <param name="Event">The event payload.</param>
/// <param name="Metadata">Optional metadata attached to the event.</param>
public sealed record EventEnvelope(
    Guid StreamId,
    int Version,
    DateTime CreatedUtc,
    IEvent Event,
    IReadOnlyDictionary<string, string>? Metadata);

/// <summary>
/// Serializes and deserializes events to and from the underlying storage representation.
/// </summary>
public interface IEventSerializer
{
    /// <summary>
    /// Serializes an event and returns the payload string while emitting the resolved event type name.
    /// </summary>
    /// <param name="event">The event instance to serialize.</param>
    /// <param name="type">Outputs the logical event type used for deserialization.</param>
    string Serialize(IEvent @event, out string type);

    /// <summary>
    /// Deserializes the provided payload into an event instance.
    /// </summary>
    /// <param name="payload">Serialized event data.</param>
    /// <param name="type">The logical event type.</param>
    IEvent Deserialize(string payload, string type);
}

/// <summary>
/// Persistence boundary responsible for storing and retrieving event streams.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends events to the stream enforcing optimistic concurrency using <paramref name="expectedVersion"/>.
    /// </summary>
    Task AppendAsync(Guid streamId, int expectedVersion, IEnumerable<IEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads events from the stream starting at <paramref name="fromVersion"/>.
    /// </summary>
    Task<IReadOnlyList<EventEnvelope>> ReadStreamAsync(Guid streamId, int fromVersion = 0, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contract for snapshot persistence used to speed up aggregate loading.
/// </summary>
public interface ISnapshotStore
{
    /// <summary>
    /// Attempts to retrieve a snapshot for the specified stream.
    /// </summary>
    Task<(bool found, int version, byte[] data)> TryGetAsync(Guid streamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a snapshot for the specified stream.
    /// </summary>
    Task SaveAsync(Guid streamId, int version, byte[] data, CancellationToken cancellationToken = default);
}

/// <summary>
/// Exception thrown when optimistic concurrency rules are violated.
/// </summary>
public class ConcurrencyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
    /// </summary>
    public ConcurrencyException(string message) : base(message) { }
}

/// <summary>
/// Repository abstraction that hydrates aggregates from the event store.
/// </summary>
public interface IEventSourcedRepository<TAggregate>
    where TAggregate : AggregateRoot, new()
{
    /// <summary>
    /// Loads an aggregate identified by <paramref name="id"/> from the event store.
    /// </summary>
    Task<TAggregate> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the aggregate changes back to the event store.
    /// </summary>
    Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}
