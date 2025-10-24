using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Quasar.Identity.Persistence.Relational.EfCore.Migrations;

[DbContext(typeof(Quasar.Identity.Persistence.Relational.EfCore.IdentityReadModelContext))]
public class IdentityReadModelContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity<Quasar.Identity.Persistence.Relational.EfCore.IdentityUserReadModel>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Quasar.Identity.Persistence.Relational.EfCore.IdentitySessionReadModel>(e =>
        {
            e.ToTable("Sessions");
            e.HasKey(x => x.SessionId);
        });
    }
}

