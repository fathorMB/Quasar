using System.Threading;
using System.Threading.Tasks;
using Quasar.Persistence.Abstractions;
using Quasar.Projections.Abstractions;
using Quasar.RealTime;
using Quasar.Samples.BasicApi.RealTime;

namespace Quasar.Samples.BasicApi;

public sealed class SensorRealTimeProjection : IProjection<SensorReadingRecorded>
{
    private readonly RealTimeProjection<SensorReadingRecorded, SensorReadingPayload> _inner;

    public SensorRealTimeProjection(
        ITimeSeriesWriter writer,
        SensorTimeSeriesAdapter seriesAdapter,
        IRealTimeNotifier<SensorReadingPayload> notifier,
        SensorPayloadAdapter payloadAdapter)
    {
        _inner = new RealTimeProjection<SensorReadingRecorded, SensorReadingPayload>(SensorConstants.MetricName, writer, seriesAdapter, notifier, payloadAdapter);
    }

    public Task HandleAsync(SensorReadingRecorded @event, CancellationToken cancellationToken = default)
        => _inner.HandleAsync(@event, cancellationToken);
}
