using Quasar.Persistence.Abstractions;

namespace Quasar.RealTime;

public sealed class DelegateTimeSeriesEventAdapter<TEvent> : ITimeSeriesEventAdapter<TEvent>
{
    private readonly Func<TEvent, IEnumerable<TimeSeriesPoint>> _map;

    public DelegateTimeSeriesEventAdapter(Func<TEvent, IEnumerable<TimeSeriesPoint>> map)
    {
        _map = map;
    }

    public IEnumerable<TimeSeriesPoint> Map(TEvent @event) => _map(@event);
}

public sealed class DelegateRealTimePayloadAdapter<TEvent, TPayload> : IRealTimePayloadAdapter<TEvent, TPayload>
{
    private readonly Func<TEvent, TPayload> _map;

    public DelegateRealTimePayloadAdapter(Func<TEvent, TPayload> map)
    {
        _map = map;
    }

    public TPayload Map(TEvent @event) => _map(@event);
}
