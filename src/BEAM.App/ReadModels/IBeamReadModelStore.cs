using Quasar.Persistence.Abstractions;
using Quasar.Persistence.Relational.EfCore;
using Microsoft.EntityFrameworkCore;
using BEAM.App.Domain.Devices;

namespace BEAM.App.ReadModels;

/// <summary>
/// Marker class for BEAM application read model store.
/// </summary>
public sealed class BeamReadModelStore : IReadModelStoreMarker
{
}

/// <summary>
/// Defines entity configurations for BEAM read models.
/// </summary>
public sealed class BeamReadModelDefinition : ReadModelDefinition<BeamReadModelStore>
{
    public override void Configure(ModelBuilder builder)
    {
        builder.Entity<BEAM.App.Domain.Devices.DeviceReadModel>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable("Devices");
            e.Property(x => x.DeviceName).HasMaxLength(100).IsRequired();
            e.Property(x => x.DeviceType).HasMaxLength(50).IsRequired();
            e.Property(x => x.MacAddress).HasMaxLength(17).IsRequired();
        });
    }
}
