namespace Quasar.RealTime;

public interface IRealTimeNotifier<TPayload>
{
    Task NotifyAsync(TPayload payload, CancellationToken cancellationToken = default);
}
