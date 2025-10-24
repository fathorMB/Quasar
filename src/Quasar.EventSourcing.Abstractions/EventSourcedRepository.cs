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

    /// <summary>
    /// Initializes a new instance of the <see cref="EventSourcedRepository{TAggregate}"/> class.
    /// </summary>
    /// <param name="store">The event store used for persistence.</param>
    public EventSourcedRepository(IEventStore store)
    {
        _store = store;
    }

    /// <inheritdoc />
    public async Task<TAggregate> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var events = await _store.ReadStreamAsync(id, 0, cancellationToken).ConfigureAwait(false);
        var aggregate = new TAggregate();
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
        await _store.AppendAsync(aggregate.Id, expectedVersion, uncommitted.Cast<IEvent>(), cancellationToken)
            .ConfigureAwait(false);
    }
}
