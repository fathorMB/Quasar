using System;
using Quartz;

namespace Quasar.Scheduling.Quartz;

internal sealed class JobHistoryListener : IJobListener
{
    private readonly JobExecutionHistoryStore _store;

    public JobHistoryListener(JobExecutionHistoryStore store)
    {
        _store = store;
    }

    public string Name => "QuasarJobHistoryListener";

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        // Nothing to do before execution for now.
        return Task.CompletedTask;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        // Not recording vetoed executions for now.
        return Task.CompletedTask;
    }

    public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        var record = new JobExecutionRecord(
            JobName: context.JobDetail.Key.Name,
            JobGroup: context.JobDetail.Key.Group,
            TriggerName: context.Trigger.Key.Name,
            TriggerGroup: context.Trigger.Key.Group,
            ScheduledFireTimeUtc: context.ScheduledFireTimeUtc?.UtcDateTime,
            FireTimeUtc: context.FireTimeUtc.UtcDateTime,
            NextFireTimeUtc: context.NextFireTimeUtc?.UtcDateTime,
            EndTimeUtc: DateTimeOffset.UtcNow,
            Success: jobException is null,
            Error: jobException?.Message);

        _store.Add(record);
        return Task.CompletedTask;
    }
}
