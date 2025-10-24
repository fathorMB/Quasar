using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;

namespace Quasar.Logging;

public static class LoggingExtensions
{
    public static IHostBuilder UseQuasarSerilog(this IHostBuilder builder, Action<LoggingOptions>? configure = null)
    {
        var options = new LoggingOptions();
        configure?.Invoke(options);

        return builder.UseSerilog((context, services, configuration) =>
        {
            configuration.MinimumLevel.Is(options.MinimumLevel);
            if (options.UseConsole)
            {
                configuration.WriteTo.Console();
            }
            if (options.UseFile && !string.IsNullOrWhiteSpace(options.FilePath))
            {
                configuration.WriteTo.File(options.FilePath,
                    rollingInterval: options.FileRollingInterval,
                    retainedFileCountLimit: 31,
                    shared: true);
            }
            if (options.UseSeq)
            {
                configuration.WriteTo.Seq(options.SeqUrl, apiKey: options.SeqApiKey);
            }
        }, writeToProviders: true);
    }
}
