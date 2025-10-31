using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quasar.Sagas.Persistence.Relational.EfCore;

internal sealed class SagaDbContext : DbContext
{
    public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options)
    {
    }

    public DbSet<SagaRecord> Sagas => Set<SagaRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        EntityTypeBuilder<SagaRecord> entity = modelBuilder.Entity<SagaRecord>();
        entity.ToTable("QuasarSagaStates");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();
        entity.Property(x => x.SagaType).IsRequired().HasMaxLength(256);
        entity.Property(x => x.StateType).IsRequired().HasMaxLength(256);
        entity.Property(x => x.UpdatedUtc).IsRequired();
        entity.Property(x => x.Payload).IsRequired();
        entity.HasIndex(x => x.SagaType);
        entity.HasIndex(x => x.StateType);
    }
}
