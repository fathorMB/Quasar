namespace Quasar.Sagas;

/// <summary>
/// Convenience base class implementing <see cref="ISagaState"/>.
/// </summary>
public abstract class SagaState : ISagaState
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <inheritdoc />
    public bool IsCompleted { get; set; }

    /// <inheritdoc />
    public DateTimeOffset UpdatedUtc { get; set; } = DateTimeOffset.UtcNow;
}
