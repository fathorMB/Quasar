namespace Quasar.EventSourcing.Outbox.EfCore;

public class InboxMessageEntity
{
    public string Source { get; set; } = default!;
    public string MessageId { get; set; } = default!;
    public DateTime ReceivedUtc { get; set; }
    public string? Hash { get; set; }
    public DateTime? ProcessedUtc { get; set; }
}
