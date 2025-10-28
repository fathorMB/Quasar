using Microsoft.EntityFrameworkCore;

namespace Quasar.EventSourcing.Outbox.EfCore;

/// <summary>
/// EF Core context used to persist outbox and inbox messages.
/// </summary>
public class OutboxDbContext : DbContext
{
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options)
    {
    }

    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    public DbSet<InboxMessageEntity> InboxMessages => Set<InboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDbContext).Assembly);
    }
}
