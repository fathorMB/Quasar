using Microsoft.AspNetCore.SignalR;
using Quasar.RealTime.Notifications;

namespace Quasar.RealTime.SignalR.Notifications;

/// <summary>
/// Payload sent to browser clients via SignalR when a notification is dispatched.
/// Includes <see cref="Id"/> so the frontend can de-duplicate against persisted notifications.
/// </summary>
public record NotificationPayload(
    Guid Id,
    string Title,
    string Message,
    string Type,
    DateTime CreatedAt
);

/// <summary>
/// Strongly-typed client contract for the Notification SignalR hub.
/// </summary>
public interface INotificationClient
{
    Task ReceiveNotification(NotificationPayload notification);
}

/// <summary>
/// Framework-provided SignalR hub for user-targeted notifications.
/// Consumer apps map this hub via <c>MapNotificationHub()</c>.
/// </summary>
public class NotificationHub : RealTimeHub<INotificationClient>
{
}
