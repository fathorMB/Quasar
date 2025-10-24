namespace Quasar.Core;

/// <summary>
/// Provides helpers for generating identifier values that use <see cref="Guid"/> as the backing type.
/// </summary>
public static class GuidIds
{
    /// <summary>
    /// Generates a new random identifier using <see cref="Guid.NewGuid"/>.
    /// </summary>
    /// <returns>A new globally unique identifier value.</returns>
    public static Guid New() => Guid.NewGuid();
}
