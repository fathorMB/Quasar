using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quasar.EventSourcing.Outbox.EfCore;

internal sealed class InboxMessageEntityConfiguration : IEntityTypeConfiguration<InboxMessageEntity>
{
    public void Configure(EntityTypeBuilder<InboxMessageEntity> builder)
    {
        builder.ToTable("QuasarInboxMessages");
        builder.HasKey(x => new { x.Source, x.MessageId });

        builder.Property(x => x.Source)
            .HasMaxLength(128);

        builder.Property(x => x.MessageId)
            .HasMaxLength(256);

        builder.Property(x => x.Hash)
            .HasMaxLength(512);

        builder.Property(x => x.ReceivedUtc)
            .IsRequired();

        builder.HasIndex(x => x.ReceivedUtc);
    }
}
