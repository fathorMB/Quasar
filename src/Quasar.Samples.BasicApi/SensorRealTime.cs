using System.Collections.Generic;
using Quasar.RealTime;
using Quasar.Persistence.Abstractions;
using Quasar.Samples.BasicApi;

namespace Quasar.Samples.BasicApi.RealTime;

public sealed record SensorReadingPayload(Guid DeviceId, string SensorType, double Value, DateTime TimestampUtc);

public sealed class SensorTimeSeriesAdapter : ITimeSeriesEventAdapter<SensorReadingRecorded>
{
    public IEnumerable<TimeSeriesPoint> Map(SensorReadingRecorded @event)
    {
        yield return new TimeSeriesPoint(
            @event.TimestampUtc,
            new Dictionary<string, string>
            {
                ["deviceId"] = @event.DeviceId.ToString(),
                ["sensorType"] = @event.SensorType
            },
            new Dictionary<string, double>
            {
                ["value"] = @event.Value
            });
    }
}

public sealed class SensorPayloadAdapter : IRealTimePayloadAdapter<SensorReadingRecorded, SensorReadingPayload>
{
    public SensorReadingPayload Map(SensorReadingRecorded @event)
        => new(@event.DeviceId, @event.SensorType, @event.Value, @event.TimestampUtc);
}
