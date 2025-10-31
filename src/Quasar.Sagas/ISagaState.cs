namespace Quasar.Sagas;

/// <summary>
/// Represents persisted saga state.
/// </summary>
public interface ISagaState
{
    /// <summary>
    /// Gets or sets the saga identifier.
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the saga has completed.
    /// </summary>
    bool IsCompleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp for the last mutation.
    /// </summary>
    DateTimeOffset UpdatedUtc { get; set; }
}
