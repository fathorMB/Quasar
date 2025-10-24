using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;
using System.IO;

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
            foreach (var kv in options.LevelOverrides)
            {
                configuration.MinimumLevel.Override(kv.Key, kv.Value);
            }
            if (options.UseConsole)
            {
                configuration.WriteTo.Console();
            }
            if (options.UseFile && !string.IsNullOrWhiteSpace(options.FilePath))
            {
                var dir = Path.GetDirectoryName(options.FilePath);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    Directory.CreateDirectory(dir);
                }
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

    public static IHostBuilder UseQuasarSerilog(this IHostBuilder builder, IConfiguration configuration, string sectionName = "Logging", Action<LoggingOptions>? configure = null)
    {
        return builder.UseQuasarSerilog(options =>
        {
            configuration?.GetSection(sectionName)?.Bind(options);
            configure?.Invoke(options);
        });
    }
}
