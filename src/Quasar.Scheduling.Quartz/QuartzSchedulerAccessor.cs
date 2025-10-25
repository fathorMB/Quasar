using Quartz;

namespace Quasar.Scheduling.Quartz;

/// <summary>
/// Provides access to the singleton Quartz scheduler managed by Quasar.
/// </summary>
public interface IQuartzSchedulerAccessor
{
    /// <summary>
    /// Gets the scheduler instance. Becomes available after the hosted service has started.
    /// </summary>
    IScheduler? Scheduler { get; }

    /// <summary>
    /// Updates the scheduler reference.
    /// </summary>
    void SetScheduler(IScheduler? scheduler);
}

internal sealed class QuartzSchedulerAccessor : IQuartzSchedulerAccessor
{
    public IScheduler? Scheduler { get; private set; }

    public void SetScheduler(IScheduler? scheduler) => Scheduler = scheduler;
}
