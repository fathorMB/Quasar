using System.Collections.Specialized;

namespace Quasar.Scheduling.Quartz;

/// <summary>
/// Configuration options used to build and run a Quartz scheduler within Quasar.
/// </summary>
public sealed class QuartzSchedulerOptions
{
    /// <summary>
    /// Gets or sets the scheduler instance name.
    /// </summary>
    public string SchedulerName { get; set; } = "quasar-scheduler";

    /// <summary>
    /// Gets the Quartz factory properties used to configure the underlying <see cref="Quartz.IScheduler"/>.
    /// </summary>
    public NameValueCollection FactoryProperties { get; } = new();

    /// <summary>
    /// Gets or sets an optional callback used to register jobs and triggers on the scheduler.
    /// </summary>
    public Action<QuartzSchedulerBuilder>? Configure { get; set; }
        = null;
}
