using Microsoft.EntityFrameworkCore;
using Quasar.RealTime.Notifications;

namespace Quasar.Persistence.Relational.EfCore;

public class NotificationReadModelDefinition<TStore> : ReadModelDefinition<TStore> 
    where TStore : class, IReadModelStoreMarker
{
    public override void Configure(ModelBuilder builder)
    {
        builder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            
            // Default table name convention would be Notifications or similar
            // We can let convention decide or force table name
            // entity.ToTable("PlayerNotifications"); // Better to stick to convention or Quasar standard
        });
    }
}
