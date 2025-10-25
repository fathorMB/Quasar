using System.Collections.Specialized;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;

namespace Quasar.Scheduling.Quartz;

internal sealed class QuartzSchedulerHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly QuartzSchedulerOptions _options;
    private readonly IQuartzSchedulerAccessor _accessor;
    private readonly ILogger<QuartzSchedulerHostedService> _logger;
    private IScheduler? _scheduler;

    public QuartzSchedulerHostedService(
        IServiceProvider services,
        IOptions<QuartzSchedulerOptions> options,
        IQuartzSchedulerAccessor accessor,
        ILogger<QuartzSchedulerHostedService> logger)
    {
        _services = services;
        _options = options.Value;
        _accessor = accessor;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var properties = new NameValueCollection(_options.FactoryProperties)
        {
            ["quartz.scheduler.instanceName"] = _options.SchedulerName
        };

        var factory = new StdSchedulerFactory(properties);
        _scheduler = await factory.GetScheduler(cancellationToken).ConfigureAwait(false);
        _scheduler.JobFactory = new ServiceProviderJobFactory(_services);
        _accessor.SetScheduler(_scheduler);

        var builder = new QuartzSchedulerBuilder();
        _options.Configure?.Invoke(builder);
        foreach (var registration in builder.Registrations)
        {
            await registration(_scheduler, _services, cancellationToken).ConfigureAwait(false);
        }

        await _scheduler.Start(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Quartz scheduler '{Scheduler}' started", _options.SchedulerName);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_scheduler is null) return;
        await _scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken).ConfigureAwait(false);
        _accessor.SetScheduler(null);
        _logger.LogInformation("Quartz scheduler '{Scheduler}' stopped", _options.SchedulerName);
    }
}
