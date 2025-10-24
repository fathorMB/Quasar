using Microsoft.Extensions.Hosting;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.Projections.Abstractions;

public interface IProjection<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

public interface ICheckpointStore
{
    Task<long> GetCheckpointAsync(string projectorName, CancellationToken cancellationToken = default);
    Task SaveCheckpointAsync(string projectorName, long position, CancellationToken cancellationToken = default);
}

public interface IProjector
{
    Task ProjectAsync(EventEnvelope envelope, CancellationToken cancellationToken = default);
}
