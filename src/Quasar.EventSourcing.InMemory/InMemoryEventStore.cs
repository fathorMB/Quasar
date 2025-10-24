using System.Collections.Concurrent;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.InMemory;

/// <summary>
/// Simple in-memory event store implementation intended for tests and demos.
/// </summary>
public sealed class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentDictionary<Guid, List<EventEnvelope>> _streams = new();
    private readonly object _gate = new();

    /// <inheritdoc />
    public Task AppendAsync(Guid streamId, int expectedVersion, IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var list = _streams.GetOrAdd(streamId, _ => new List<EventEnvelope>());
            var currentVersion = list.Count == 0 ? 0 : list[^1].Version;
            if (currentVersion != expectedVersion)
                throw new ConcurrencyException($"Stream {streamId} expected version {expectedVersion} but was {currentVersion}.");

            var now = DateTime.UtcNow;
            var version = currentVersion;
            foreach (var e in events)
            {
                list.Add(new EventEnvelope(streamId, ++version, now, e, null));
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<EventEnvelope>> ReadStreamAsync(Guid streamId, int fromVersion = 0, CancellationToken cancellationToken = default)
    {
        if (!_streams.TryGetValue(streamId, out var list))
            return Task.FromResult<IReadOnlyList<EventEnvelope>>(Array.Empty<EventEnvelope>());

        var result = list.Where(e => e.Version > fromVersion).OrderBy(e => e.Version).ToArray();
        return Task.FromResult<IReadOnlyList<EventEnvelope>>(result);
    }
}
