using Microsoft.EntityFrameworkCore;
using Quasar.Persistence.Relational.EfCore;

namespace Quasar.Samples.BasicApi;

public sealed class CounterReadModel
{
    public Guid Id { get; set; }
    public int Count { get; set; }
}

public sealed class CartReadModel
{
    public Guid Id { get; set; }
    public int TotalItems { get; set; }
}

public sealed class CartProductLine
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public sealed class SampleReadModelContext : ReadModelContext
{
    public SampleReadModelContext(DbContextOptions<SampleReadModelContext> options) : base(options) { }
    public DbSet<CounterReadModel> Counters => Set<CounterReadModel>();
    public DbSet<CartReadModel> Carts => Set<CartReadModel>();
    public DbSet<CartProductLine> CartProducts => Set<CartProductLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CounterReadModel>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable("Counters");
        });

        modelBuilder.Entity<CartReadModel>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable("Carts");
        });

        modelBuilder.Entity<CartProductLine>(e =>
        {
            e.HasKey(x => x.ProductId);
            e.ToTable("CartProducts");
        });
    }
}
