using Quasar.Domain;

namespace Quasar.EventSourcing.Abstractions;

public sealed class EventSourcedRepository<TAggregate> : IEventSourcedRepository<TAggregate>
    where TAggregate : AggregateRoot, new()
{
    private readonly IEventStore _store;

    public EventSourcedRepository(IEventStore store)
    {
        _store = store;
    }

    public async Task<TAggregate> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var events = await _store.ReadStreamAsync(id, 0, cancellationToken).ConfigureAwait(false);
        var aggregate = new TAggregate();
        if (events.Count == 0)
            return aggregate;

        aggregate.Rehydrate(events.Select(e => e.Event));
        return aggregate;
    }

    public async Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var uncommitted = aggregate.DequeueUncommitted();
        if (uncommitted.Count == 0) return;

        var expectedVersion = aggregate.Version - uncommitted.Count;
        await _store.AppendAsync(aggregate.Id, expectedVersion, uncommitted.Cast<IEvent>(), cancellationToken)
            .ConfigureAwait(false);
    }
}

