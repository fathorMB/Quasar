using System.Linq;
using Quasar.EventSourcing.Abstractions;
using Quasar.Persistence.Abstractions;
using Quasar.Projections.Abstractions;

namespace Quasar.RealTime;

public interface ITimeSeriesEventAdapter<in TEvent>
{
    IEnumerable<TimeSeriesPoint> Map(TEvent @event);
}

public interface IRealTimePayloadAdapter<in TEvent, TPayload>
{
    TPayload Map(TEvent @event);
}

public sealed class RealTimeProjection<TEvent, TPayload> : IProjection<TEvent>
    where TEvent : IEvent
{
    private readonly ITimeSeriesWriter _writer;
    private readonly ITimeSeriesEventAdapter<TEvent> _seriesAdapter;
    private readonly IRealTimeNotifier<TPayload> _notifier;
    private readonly IRealTimePayloadAdapter<TEvent, TPayload> _payloadAdapter;
    private readonly string _metric;

    public RealTimeProjection(
        string metric,
        ITimeSeriesWriter writer,
        ITimeSeriesEventAdapter<TEvent> seriesAdapter,
        IRealTimeNotifier<TPayload> notifier,
        IRealTimePayloadAdapter<TEvent, TPayload> payloadAdapter)
    {
        _metric = metric;
        _writer = writer;
        _seriesAdapter = seriesAdapter;
        _notifier = notifier;
        _payloadAdapter = payloadAdapter;
    }

    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        var points = _seriesAdapter.Map(@event).ToArray();
        if (points.Length > 0)
        {
            await _writer.WriteAsync(_metric, points, cancellationToken).ConfigureAwait(false);
        }

        var payload = _payloadAdapter.Map(@event);
        await _notifier.NotifyAsync(payload, cancellationToken).ConfigureAwait(false);
    }
}
