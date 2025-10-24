using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Quasar.Persistence.TimeSeries.Timescale;

/// <summary>
/// Hosted service that eagerly prepares TimescaleDB schema on application startup.
/// </summary>
internal sealed class TimescaleInitializerHostedService : IHostedService
{
    private readonly TimescaleOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimescaleInitializerHostedService"/> class.
    /// </summary>
    public TimescaleInitializerHostedService(IOptions<TimescaleOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
        => TimescaleInitializer.EnsureSchemaAsync(_options, cancellationToken);

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
