namespace Quasar.EventSourcing.Outbox.EfCore;

public class OutboxMessageEntity
{
    public Guid MessageId { get; set; }
    public Guid StreamId { get; set; }
    public int StreamVersion { get; set; }
    public string EventName { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime CreatedUtc { get; set; }
    public string? Destination { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime? DispatchedUtc { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public string? LastError { get; set; }
}
