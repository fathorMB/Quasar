using Quasar.Core;
using Quasar.Domain;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.Outbox;

/// <summary>
/// Builds outbox messages by serializing domain events with <see cref="IEventSerializer"/>.
/// </summary>
public sealed class DefaultOutboxMessageFactory : IOutboxMessageFactory
{
    private readonly IEventSerializer _serializer;
    private readonly IClock _clock;

    public DefaultOutboxMessageFactory(IEventSerializer serializer, IClock clock)
    {
        _serializer = serializer;
        _clock = clock;
    }

    public IReadOnlyList<OutboxMessage> Create(Guid streamId, int startingVersion, IReadOnlyList<IDomainEvent> events)
    {
        if (events.Count == 0)
        {
            return Array.Empty<OutboxMessage>();
        }

        var createdUtc = _clock.UtcNow;
        var messages = new List<OutboxMessage>(events.Count);

        for (var i = 0; i < events.Count; i++)
        {
            if (events[i] is not IEvent eventPayload)
            {
                continue;
            }

            var payload = _serializer.Serialize(eventPayload, out var eventName);
            var version = startingVersion + i + 1;

            messages.Add(new OutboxMessage(
                MessageId: Guid.NewGuid(),
                StreamId: streamId,
                StreamVersion: version,
                Event: eventPayload,
                CreatedUtc: createdUtc,
                EventName: eventName,
                Payload: payload));
        }

        return messages;
    }
}
