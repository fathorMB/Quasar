using Quartz;

namespace Quasar.Scheduling.Quartz;

/// <summary>
/// Fluent builder used to register jobs and triggers with a Quartz scheduler.
/// </summary>
public sealed class QuartzSchedulerBuilder
{
    private readonly List<Func<IScheduler, IServiceProvider, CancellationToken, Task>> _registrations = new();

    internal IReadOnlyList<Func<IScheduler, IServiceProvider, CancellationToken, Task>> Registrations => _registrations;

    /// <summary>
    /// Registers a custom scheduling delegate that is executed when the scheduler starts.
    /// </summary>
    public QuartzSchedulerBuilder Schedule(Func<IScheduler, IServiceProvider, CancellationToken, Task> registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        _registrations.Add(registration);
        return this;
    }

    /// <summary>
    /// Convenience helper to schedule a job of type <typeparamref name="TJob"/> using the supplied builders.
    /// </summary>
    public QuartzSchedulerBuilder ScheduleJob<TJob>(Func<JobBuilder, JobBuilder> configureJob, Func<TriggerBuilder, TriggerBuilder> configureTrigger)
        where TJob : IJob
    {
        ArgumentNullException.ThrowIfNull(configureJob);
        ArgumentNullException.ThrowIfNull(configureTrigger);

        return Schedule(async (scheduler, services, cancellationToken) =>
        {
            var jobBuilder = configureJob(JobBuilder.Create<TJob>());
            var job = jobBuilder.Build();

            if (await scheduler.CheckExists(job.Key, cancellationToken).ConfigureAwait(false)) return;

            var triggerBuilder = configureTrigger(TriggerBuilder.Create().ForJob(job));
            var trigger = triggerBuilder.Build();

            await scheduler.ScheduleJob(job, trigger, cancellationToken).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Convenience helper to schedule a Quasar job that does not take an explicit dependency on Quartz abstractions.
    /// </summary>
    public QuartzSchedulerBuilder ScheduleQuasarJob<TJob>(Func<JobBuilder, JobBuilder> configureJob, Func<TriggerBuilder, TriggerBuilder> configureTrigger)
        where TJob : class, IQuasarJob
    {
        ArgumentNullException.ThrowIfNull(configureJob);
        ArgumentNullException.ThrowIfNull(configureTrigger);

        return Schedule(async (scheduler, services, cancellationToken) =>
        {
            var jobBuilder = configureJob(JobBuilder.Create<QuasarJobAdapter<TJob>>());
            var job = jobBuilder.Build();

            if (await scheduler.CheckExists(job.Key, cancellationToken).ConfigureAwait(false)) return;

            var triggerBuilder = configureTrigger(TriggerBuilder.Create().ForJob(job));
            var trigger = triggerBuilder.Build();

            await scheduler.ScheduleJob(job, trigger, cancellationToken).ConfigureAwait(false);
        });
    }
}
