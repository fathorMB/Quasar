namespace BEAM.App.Models;

/// <summary>
/// Configuration data sent to devices via SignalR.
/// </summary>
public record DeviceConfiguration
{
    /// <summary>
    /// Updated device name.
    /// </summary>
    public string? DeviceName { get; init; }

    /// <summary>
    /// Updated heartbeat interval in seconds.
    /// </summary>
    public int? HeartbeatIntervalSeconds { get; init; }

    /// <summary>
    /// Timestamp when the configuration was updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }
}
