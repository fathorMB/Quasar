using Microsoft.EntityFrameworkCore;
using Quasar.Persistence.Relational.EfCore;

namespace Quasar.Samples.BasicApi;

public sealed class SampleReadModelStore : IReadModelStoreMarker
{
}

public sealed class SampleReadModelDefinition : ReadModelDefinition<SampleReadModelStore>
{
    public override void Configure(ModelBuilder builder)
    {
        builder.Entity<CounterReadModel>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable("Counters");
        });

        builder.Entity<CartReadModel>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable("Carts");
        });

        builder.Entity<CartProductLine>(e =>
        {
            e.HasKey(x => x.ProductId);
            e.ToTable("CartProducts");
        });
    }
}
