using System.Linq;
using Quasar.EventSourcing.Abstractions;
using Quasar.Persistence.Abstractions;
using Quasar.Projections.Abstractions;

namespace Quasar.RealTime;

/// <summary>
/// Maps events into time series points suitable for persistence.
/// </summary>
public interface ITimeSeriesEventAdapter<in TEvent>
{
    IEnumerable<TimeSeriesPoint> Map(TEvent @event);
}

/// <summary>
/// Maps events into payloads sent to real-time subscribers.
/// </summary>
public interface IRealTimePayloadAdapter<in TEvent, TPayload>
{
    TPayload Map(TEvent @event);
}

/// <summary>
/// Projection that persists events as time series points and broadcasts a real-time payload.
/// </summary>
public sealed class RealTimeProjection<TEvent, TPayload> : IProjection<TEvent>
    where TEvent : IEvent
{
    private readonly ITimeSeriesWriter _writer;
    private readonly ITimeSeriesEventAdapter<TEvent> _seriesAdapter;
    private readonly IRealTimeNotifier<TPayload> _notifier;
    private readonly IRealTimePayloadAdapter<TEvent, TPayload> _payloadAdapter;
    private readonly string _metric;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealTimeProjection{TEvent, TPayload}"/> class.
    /// </summary>
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

    /// <inheritdoc />
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
