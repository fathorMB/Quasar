using Microsoft.EntityFrameworkCore;
using Quasar.RealTime.Notifications;

namespace Quasar.Persistence.Relational.EfCore;

public class EfCoreNotificationStore<TContext> : INotificationStore where TContext : ReadModelContext
{
    private readonly TContext _context;

    public EfCoreNotificationStore(TContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Set<Notification>().AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetUnreadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Notification>()
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        await _context.Set<Notification>()
            .Where(n => n.Id == notificationId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
    }
}
