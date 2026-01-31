namespace Quasar.RealTime.Notifications;
using System.Security.Principal;

public record Notification(
    Guid Id,
    Guid UserId,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt
);

public interface INotificationStore
{
    Task SaveAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetUnreadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task SendAsync(Guid userId, string title, string message, string type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetUnreadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
