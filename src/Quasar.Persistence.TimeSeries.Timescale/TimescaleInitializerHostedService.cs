using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Quasar.Persistence.TimeSeries.Timescale;

internal sealed class TimescaleInitializerHostedService : IHostedService
{
    private readonly TimescaleOptions _options;

    public TimescaleInitializerHostedService(IOptions<TimescaleOptions> options)
    {
        _options = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => TimescaleInitializer.EnsureSchemaAsync(_options, cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
