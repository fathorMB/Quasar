using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Quasar.Scheduling.Quartz;

internal sealed class QuasarJobAdapter<TJob> : IJob where TJob : class, IQuasarJob
{
    private readonly TJob _job;
    private readonly ILogger<QuasarJobAdapter<TJob>> _logger;

    public QuasarJobAdapter(TJob job, ILogger<QuasarJobAdapter<TJob>> logger)
    {
        _job = job;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobContext = CreateContext(context);
        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["JobName"] = jobContext.JobName,
            ["JobGroup"] = jobContext.JobGroup,
            ["TriggerName"] = jobContext.TriggerName,
            ["TriggerGroup"] = jobContext.TriggerGroup,
            ["FireInstanceId"] = jobContext.FireInstanceId
        }))
        {
            _logger.LogInformation("Starting scheduled job {Job}/{Trigger}", jobContext.JobKey(), jobContext.TriggerKey());
            try
            {
                await _job.ExecuteAsync(jobContext, context.CancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Completed scheduled job {Job}/{Trigger}", jobContext.JobKey(), jobContext.TriggerKey());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled job {Job}/{Trigger} failed", jobContext.JobKey(), jobContext.TriggerKey());
                throw;
            }
        }
    }

    private static QuasarJobContext CreateContext(IJobExecutionContext context)
    {
        var data = context.MergedJobDataMap?
            .ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value)
            ?? new Dictionary<string, object?>();

        return new QuasarJobContext(
            context.JobDetail.Key.Name,
            context.JobDetail.Key.Group,
            context.Trigger.Key.Name,
            context.Trigger.Key.Group,
            context.FireInstanceId,
            context.ScheduledFireTimeUtc?.UtcDateTime,
            context.FireTimeUtc.UtcDateTime,
            context.PreviousFireTimeUtc?.UtcDateTime,
            context.NextFireTimeUtc?.UtcDateTime,
            data);
    }
}

internal static class QuasarJobContextExtensions
{
    public static string JobKey(this QuasarJobContext context) => $"{context.JobGroup}/{context.JobName}";
    public static string TriggerKey(this QuasarJobContext context) => $"{context.TriggerGroup}/{context.TriggerName}";
}
