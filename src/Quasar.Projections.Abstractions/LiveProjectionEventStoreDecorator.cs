using Quasar.EventSourcing.Abstractions;
using Quasar.Projections.Abstractions;

namespace Quasar.Projections.Abstractions;

/// <summary>
/// Decorates an event store to automatically dispatch events to live projections
/// immediately after persistence.
/// </summary>
public sealed class LiveProjectionEventStoreDecorator : IEventStore
{
    private readonly IEventStore _innerStore;
    private readonly ILiveProjectionPipeline _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveProjectionEventStoreDecorator"/> class.
    /// </summary>
    public LiveProjectionEventStoreDecorator(IEventStore innerStore, ILiveProjectionPipeline pipeline)
    {
        _innerStore = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventEnvelope>> ReadStreamAsync(
        Guid streamId,
        int fromVersion = 0,
        CancellationToken cancellationToken = default)
    {
        return await _innerStore.ReadStreamAsync(streamId, fromVersion, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AppendAsync(
        Guid streamId,
        int expectedVersion,
        IEnumerable<IEvent> events,
        CancellationToken cancellationToken = default)
    {
        await _innerStore.AppendAsync(streamId, expectedVersion, events, cancellationToken).ConfigureAwait(false);

        // After successful persistence, dispatch events to live projections
        var eventList = events.ToList();
        int version = expectedVersion + 1;
        foreach (var evt in eventList)
        {
            var envelope = new EventEnvelope(streamId, version, DateTime.UtcNow, evt, null);
            await _pipeline.DispatchAsync(envelope, cancellationToken).ConfigureAwait(false);
            version++;
        }
    }
}
