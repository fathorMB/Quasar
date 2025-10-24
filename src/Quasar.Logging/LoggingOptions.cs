using Serilog;
using Serilog.Events;

namespace Quasar.Logging;

/// <summary>
/// Configuration options driving the default Serilog pipeline.
/// </summary>
public sealed class LoggingOptions
{
    /// <summary>
    /// Gets or sets the minimum log level emitted by the application.
    /// </summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Gets or sets a value indicating whether logs should be written to the console.
    /// </summary>
    public bool UseConsole { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether logs should be written to a file sink.
    /// </summary>
    public bool UseFile { get; set; }
    
    /// <summary>
    /// Gets or sets the file path used when <see cref="UseFile"/> is enabled.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the rolling interval applied to file sinks.
    /// </summary>
    public RollingInterval FileRollingInterval { get; set; } = RollingInterval.Day;

    /// <summary>
    /// Gets or sets a value indicating whether events should be forwarded to Seq.
    /// </summary>
    public bool UseSeq { get; set; }

    /// <summary>
    /// Gets or sets the Seq ingestion endpoint.
    /// </summary>
    public string SeqUrl { get; set; } = "http://localhost:5341";

    /// <summary>
    /// Gets or sets the Seq API key when authentication is required.
    /// </summary>
    public string? SeqApiKey { get; set; }

    /// <summary>
    /// Gets the overrides applied to specific logging namespaces.
    /// </summary>
    public Dictionary<string, LogEventLevel> LevelOverrides { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Microsoft"] = LogEventLevel.Warning,
        ["System"] = LogEventLevel.Warning
    };
}
