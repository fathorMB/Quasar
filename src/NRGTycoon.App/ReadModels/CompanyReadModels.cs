using Quasar.Persistence.Abstractions;
using Quasar.Persistence.Relational.EfCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRGTycoon.App.ReadModels;

/// <summary>
/// Marker class for NRG Tycoon read model store.
/// </summary>
public sealed class NRGTycoonReadModelStore : IReadModelStoreMarker
{
}

/// <summary>
/// Defines entity configurations for NRG Tycoon read models.
/// </summary>
public sealed class NRGTycoonReadModelDefinition : ReadModelDefinition<NRGTycoonReadModelStore>
{
    public override void Configure(ModelBuilder builder)
    {
        builder.Entity<CompanyReadModel>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable("Companies");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Balance).HasPrecision(18, 2);
            e.Property(x => x.Oil).HasPrecision(18, 2);
            e.Property(x => x.Gas).HasPrecision(18, 2);
            e.Property(x => x.Uranium).HasPrecision(18, 2);
        });

        builder.Entity<BalanceMovementReadModel>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable("BalanceMovements");
            e.Property(x => x.Description).HasMaxLength(500).IsRequired();
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasIndex(x => x.CompanyId);
        });
    }
}

/// <summary>
/// Read model for company data.
/// </summary>
public class CompanyReadModel
{
    public Guid Id { get; set; }
    
    public Guid OwnerId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public decimal Balance { get; set; }
    
    public decimal Oil { get; set; }
    
    public decimal Gas { get; set; }
    
    public decimal Uranium { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Read model for balance movements.
/// </summary>
public class BalanceMovementReadModel
{
    public Guid Id { get; set; }
    
    public Guid CompanyId { get; set; }
    
    public decimal Amount { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public DateTimeOffset Timestamp { get; set; }
}
