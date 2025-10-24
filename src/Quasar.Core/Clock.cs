namespace Quasar.Core;

/// <summary>
/// Provides an abstraction over retrieving the current <see cref="DateTime.UtcNow"/>. Useful for testing scenarios
/// where deterministic time is required.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current point in time expressed as Coordinated Universal Time (UTC).
    /// </summary>
    DateTime UtcNow { get; }
}

/// <summary>
/// System backed clock implementation that simply returns <see cref="DateTime.UtcNow"/>.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
