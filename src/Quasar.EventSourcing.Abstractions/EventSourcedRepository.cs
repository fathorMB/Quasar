using System.Linq;
using Quasar.Domain;

namespace Quasar.EventSourcing.Abstractions;

/// <summary>
/// Default repository implementation that hydrates aggregates from an <see cref="IEventStore"/> and persists new events.
/// </summary>
/// <typeparam name="TAggregate">Aggregate type that derives from <see cref="AggregateRoot"/>.</typeparam>
public sealed class EventSourcedRepository<TAggregate> : IEventSourcedRepository<TAggregate>
    where TAggregate : AggregateRoot, new()
{
    private readonly IEventStore _store;
    private readonly IOutboxStore? _outboxStore;
    private readonly IOutboxMessageFactory? _messageFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventSourcedRepository{TAggregate}"/> class.
    /// </summary>
    /// <param name="store">The event store used for persistence.</param>
    /// <param name="outboxStore">Optional outbox store used to capture integration messages.</param>
    /// <param name="messageFactory">Optional factory used to transform events into outbox messages.</param>
    public EventSourcedRepository(IEventStore store, IOutboxStore? outboxStore = null, IOutboxMessageFactory? messageFactory = null)
    {
        _store = store;
        _outboxStore = outboxStore;
        _messageFactory = messageFactory;
    }

    /// <inheritdoc />
    public async Task<TAggregate> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var events = await _store.ReadStreamAsync(id, 0, cancellationToken).ConfigureAwait(false);
        var aggregate = new TAggregate();
        aggregate.SetId(id);
        if (events.Count == 0)
            return aggregate;

        aggregate.Rehydrate(events.Select(e => e.Event));
        return aggregate;
    }

    /// <inheritdoc />
    public async Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var uncommitted = aggregate.DequeueUncommitted();
        if (uncommitted.Count == 0) return;

        var expectedVersion = aggregate.Version - uncommitted.Count;
        var events = uncommitted.Cast<IEvent>().ToList();

        await _store.AppendAsync(aggregate.Id, expectedVersion, events, cancellationToken)
            .ConfigureAwait(false);

        if (_outboxStore is not null && _messageFactory is not null)
        {
            var messages = _messageFactory.Create(aggregate.Id, expectedVersion, uncommitted);
            if (messages.Count > 0)
            {
                await _outboxStore.EnqueueAsync(messages, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
