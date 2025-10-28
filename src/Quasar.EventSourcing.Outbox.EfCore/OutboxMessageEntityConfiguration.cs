using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quasar.EventSourcing.Outbox.EfCore;

internal sealed class OutboxMessageEntityConfiguration : IEntityTypeConfiguration<OutboxMessageEntity>
{
    public void Configure(EntityTypeBuilder<OutboxMessageEntity> builder)
    {
        builder.ToTable("QuasarOutboxMessages");
        builder.HasKey(x => x.MessageId);

        builder.Property(x => x.EventName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.CreatedUtc)
            .IsRequired();

        builder.Property(x => x.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Destination)
            .HasMaxLength(256);

        builder.Property(x => x.AttemptCount)
            .HasDefaultValue(0);

        builder.Property(x => x.LastAttemptUtc);

        builder.Property(x => x.LastError)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.StreamId, x.StreamVersion }).IsUnique();
        builder.HasIndex(x => x.CreatedUtc);
        builder.HasIndex(x => x.DispatchedUtc);
    }
}
