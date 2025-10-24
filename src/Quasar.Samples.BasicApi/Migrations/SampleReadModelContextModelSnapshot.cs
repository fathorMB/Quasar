using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Quasar.Samples.BasicApi.Migrations;

[DbContext(typeof(Quasar.Samples.BasicApi.SampleReadModelContext))]
public class SampleReadModelContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity<Quasar.Samples.BasicApi.CounterReadModel>(e =>
        {
            e.ToTable("Counters");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Quasar.Samples.BasicApi.CartReadModel>(e =>
        {
            e.ToTable("Carts");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Quasar.Samples.BasicApi.CartProductLine>(e =>
        {
            e.ToTable("CartProducts");
            e.HasKey(x => x.ProductId);
        });
    }
}

