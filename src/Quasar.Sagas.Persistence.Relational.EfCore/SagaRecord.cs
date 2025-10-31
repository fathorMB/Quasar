namespace Quasar.Sagas.Persistence.Relational.EfCore;

internal sealed class SagaRecord
{
    public Guid Id { get; set; }
    public string SagaType { get; set; } = string.Empty;
    public string StateType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset UpdatedUtc { get; set; }
}
