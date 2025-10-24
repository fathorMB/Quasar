using Serilog;
using Serilog.Events;

namespace Quasar.Logging;

public sealed class LoggingOptions
{
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    public bool UseConsole { get; set; } = true;
    public bool UseFile { get; set; }
    public string? FilePath { get; set; }
    public RollingInterval FileRollingInterval { get; set; } = RollingInterval.Day;
    public bool UseSeq { get; set; }
    public string SeqUrl { get; set; } = "http://localhost:5341";
    public string? SeqApiKey { get; set; }
    public Dictionary<string, LogEventLevel> LevelOverrides { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Microsoft"] = LogEventLevel.Warning,
        ["System"] = LogEventLevel.Warning
    };
}
