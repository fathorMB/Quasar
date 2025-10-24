using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Quasar.RealTime.SignalR;

namespace Quasar.Samples.BasicApi.RealTime;

public interface ISensorClient
{
    Task ReceiveSensorReading(SensorReadingPayload payload);
}

public sealed class SensorHub : RealTimeHub<ISensorClient>
{
}

public sealed class SensorDispatcher : ISignalRDispatcher<ISensorClient, SensorReadingPayload>
{
    public Task DispatchAsync(ISensorClient client, SensorReadingPayload payload, CancellationToken cancellationToken = default)
        => client.ReceiveSensorReading(payload);
}
