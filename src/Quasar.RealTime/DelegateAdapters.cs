using Quasar.Persistence.Abstractions;

namespace Quasar.RealTime;

/// <summary>
/// Delegate-based adapter that converts events into <see cref="TimeSeriesPoint"/> instances.
/// </summary>
/// <typeparam name="TEvent">Event type being adapted.</typeparam>
public sealed class DelegateTimeSeriesEventAdapter<TEvent> : ITimeSeriesEventAdapter<TEvent>
{
    private readonly Func<TEvent, IEnumerable<TimeSeriesPoint>> _map;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateTimeSeriesEventAdapter{TEvent}"/> class.
    /// </summary>
    public DelegateTimeSeriesEventAdapter(Func<TEvent, IEnumerable<TimeSeriesPoint>> map)
    {
        _map = map;
    }

    /// <inheritdoc />
    public IEnumerable<TimeSeriesPoint> Map(TEvent @event) => _map(@event);
}

/// <summary>
/// Delegate-based adapter that converts events into real-time payloads.
/// </summary>
/// <typeparam name="TEvent">Event type being adapted.</typeparam>
/// <typeparam name="TPayload">Payload type produced.</typeparam>
public sealed class DelegateRealTimePayloadAdapter<TEvent, TPayload> : IRealTimePayloadAdapter<TEvent, TPayload>
{
    private readonly Func<TEvent, TPayload> _map;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateRealTimePayloadAdapter{TEvent, TPayload}"/> class.
    /// </summary>
    public DelegateRealTimePayloadAdapter(Func<TEvent, TPayload> map)
    {
        _map = map;
    }

    /// <inheritdoc />
    public TPayload Map(TEvent @event) => _map(@event);
}
