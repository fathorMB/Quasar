using Microsoft.EntityFrameworkCore;
using Quasar.Persistence.Relational.EfCore;

namespace Quasar.Identity.Persistence.Relational.EfCore;

public sealed class IdentityReadModelStore : IReadModelStoreMarker
{
}

public sealed class IdentityReadModelDefinition : ReadModelDefinition<IdentityReadModelStore>
{
    public override void Configure(ModelBuilder builder)
    {
        builder.Entity<IdentityUserReadModel>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable("Users");
            e.Property(x => x.Username).IsRequired();
            e.Property(x => x.Email).IsRequired();
        });

        builder.Entity<IdentitySessionReadModel>(e =>
        {
            e.HasKey(x => x.SessionId);
            e.ToTable("Sessions");
        });

        builder.Entity<IdentityRoleReadModel>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable("Roles");
            e.Property(x => x.Name).IsRequired();
        });

        builder.Entity<IdentityRolePermissionReadModel>(e =>
        {
            e.ToTable("RolePermissions");
            e.HasKey(x => new { x.RoleId, x.Permission });
        });

        builder.Entity<IdentityUserRoleReadModel>(e =>
        {
            e.ToTable("UserRoles");
            e.HasKey(x => new { x.UserId, x.RoleId });
        });
    }
}
