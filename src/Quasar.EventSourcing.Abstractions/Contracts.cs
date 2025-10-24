using Quasar.Domain;

namespace Quasar.EventSourcing.Abstractions;

public interface IEvent : Quasar.Domain.IDomainEvent { }

public sealed record EventEnvelope(
    Guid StreamId,
    int Version,
    DateTime CreatedUtc,
    IEvent Event,
    IReadOnlyDictionary<string, string>? Metadata);

public interface IEventSerializer
{
    string Serialize(IEvent @event, out string type);
    IEvent Deserialize(string payload, string type);
}

public interface IEventStore
{
    Task AppendAsync(Guid streamId, int expectedVersion, IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventEnvelope>> ReadStreamAsync(Guid streamId, int fromVersion = 0, CancellationToken cancellationToken = default);
}

public interface ISnapshotStore
{
    Task<(bool found, int version, byte[] data)> TryGetAsync(Guid streamId, CancellationToken cancellationToken = default);
    Task SaveAsync(Guid streamId, int version, byte[] data, CancellationToken cancellationToken = default);
}

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
}

public interface IEventSourcedRepository<TAggregate>
    where TAggregate : AggregateRoot, new()
{
    Task<TAggregate> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}
