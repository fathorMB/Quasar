namespace Quasar.RealTime.Notifications;

public class PersistentNotificationService : INotificationService
{
    private readonly INotificationStore _store;
    private readonly ITargetedRealTimeNotifier<Notification> _notifier;

    public PersistentNotificationService(INotificationStore store, ITargetedRealTimeNotifier<Notification> notifier)
    {
        _store = store;
        _notifier = notifier;
    }

    public async Task SendAsync(Guid userId, string title, string message, string type, CancellationToken cancellationToken = default)
    {
        var notification = new Notification(
            Id: Guid.NewGuid(),
            UserId: userId,
            Title: title,
            Message: message,
            Type: type,
            IsRead: false,
            CreatedAt: DateTime.UtcNow
        );

        await _store.SaveAsync(notification, cancellationToken);
        await _notifier.NotifyUserAsync(userId.ToString(), notification, cancellationToken);
    }

    public Task<IEnumerable<Notification>> GetUnreadAsync(Guid userId, CancellationToken cancellationToken = default) => _store.GetUnreadAsync(userId, cancellationToken);
    public Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default) => _store.MarkAsReadAsync(notificationId, cancellationToken);
    public Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default) => _store.MarkAllAsReadAsync(userId, cancellationToken);
}
