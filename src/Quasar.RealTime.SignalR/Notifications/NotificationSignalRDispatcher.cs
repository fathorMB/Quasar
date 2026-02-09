using Quasar.RealTime.Notifications;

namespace Quasar.RealTime.SignalR.Notifications;

/// <summary>
/// Maps a framework <see cref="Notification"/> to a <see cref="NotificationPayload"/>
/// and dispatches it to the target SignalR client.
/// </summary>
public class NotificationSignalRDispatcher : ISignalRDispatcher<INotificationClient, Notification>
{
    public Task DispatchAsync(INotificationClient client, Notification payload, CancellationToken cancellationToken = default)
    {
        return client.ReceiveNotification(new NotificationPayload(
            Id: payload.Id,
            Title: payload.Title,
            Message: payload.Message,
            Type: payload.Type,
            CreatedAt: payload.CreatedAt
        ));
    }
}
