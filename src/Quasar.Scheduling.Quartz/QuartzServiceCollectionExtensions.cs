using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace Quasar.Scheduling.Quartz;

/// <summary>
/// Service collection extensions for enabling Quartz scheduling.
/// </summary>
public static class QuartzServiceCollectionExtensions
{
    /// <summary>
    /// Registers and configures a Quartz scheduler hosted by Quasar.
    /// </summary>
    public static IServiceCollection AddQuartzScheduler(this IServiceCollection services, Action<QuartzSchedulerOptions> configure)
    {
        services.AddOptions<QuartzSchedulerOptions>().Configure(configure);
        services.TryAddSingleton<IQuartzSchedulerAccessor, QuartzSchedulerAccessor>();
        services.TryAddSingleton<JobExecutionHistoryStore>();
        services.TryAddTransient(typeof(QuasarJobAdapter<>));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, QuartzSchedulerHostedService>());
        return services;
    }
}


