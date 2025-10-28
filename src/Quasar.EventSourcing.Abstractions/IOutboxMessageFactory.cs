using Quasar.Domain;

namespace Quasar.EventSourcing.Abstractions;

/// <summary>
/// Builds outbox messages from the domain events emitted by an aggregate.
/// </summary>
public interface IOutboxMessageFactory
{
    /// <summary>
    /// Creates outbox messages for the supplied <paramref name="events"/>.
    /// </summary>
    /// <param name="streamId">Identifier of the aggregate stream.</param>
    /// <param name="startingVersion">Version from which <paramref name="events"/> will be appended.</param>
    /// <param name="events">The events to transform.</param>
    /// <returns>Messages ordered by their stream version.</returns>
    IReadOnlyList<OutboxMessage> Create(Guid streamId, int startingVersion, IReadOnlyList<IDomainEvent> events);
}

