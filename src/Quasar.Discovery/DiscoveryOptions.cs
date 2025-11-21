namespace Quasar.Discovery;

/// <summary>
/// Configuration options for the UDP discovery service.
/// </summary>
public sealed class DiscoveryOptions
{
    /// <summary>
    /// Gets or sets whether the discovery service is enabled.
    /// Default is false (opt-in).
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the UDP port to listen on for discovery requests.
    /// Default is 5353.
    /// </summary>
    public int Port { get; set; } = 5353;

    /// <summary>
    /// Gets or sets the service name to advertise in discovery responses.
    /// </summary>
    public string ServiceName { get; set; } = "Quasar Application";

    /// <summary>
    /// Gets or sets custom metadata to include in discovery responses.
    /// Developers can add version, description, or any other metadata.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the optional delay (in milliseconds) before sending responses.
    /// Can help prevent network flooding when many instances respond.
    /// Default is 0 (no delay).
    /// </summary>
    public int ResponseDelayMs { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether to include all registered endpoints in the response.
    /// Default is true.
    /// </summary>
    public bool IncludeEndpoints { get; set; } = true;
}
