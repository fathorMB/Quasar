namespace Quasar.RealTime;

public interface ITargetedRealTimeNotifier<TPayload> : IRealTimeNotifier<TPayload>
{
    Task NotifyUserAsync(string userId, TPayload payload, CancellationToken cancellationToken = default);
    Task NotifyGroupAsync(string groupName, TPayload payload, CancellationToken cancellationToken = default);
}
