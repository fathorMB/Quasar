using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl.Matchers;
using Quasar.Scheduling.Quartz;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quasar.Web;

/// <summary>
/// Endpoint mapping helpers for interacting with the Quartz scheduler runtime.
/// </summary>
public static class QuartzEndpointExtensions
{
    /// <summary>
    /// Maps management endpoints for the configured Quartz scheduler.
    /// </summary>
    public static IEndpointRouteBuilder MapQuartzEndpoints(this IEndpointRouteBuilder endpoints, string prefix = "/quartz")
    {
        endpoints.MapGet(prefix + "/jobs", ListQuartzJobs).WithName("ListQuartzJobs").WithTags("Quartz");
        endpoints.MapPost(prefix + "/jobs/{group}/{name}/trigger", TriggerQuartzJob).WithName("TriggerQuartzJob").WithTags("Quartz");
        endpoints.MapPost(prefix + "/jobs/{group}/{name}/pause", PauseQuartzJob).WithName("PauseQuartzJob").WithTags("Quartz");
        endpoints.MapPost(prefix + "/jobs/{group}/{name}/resume", ResumeQuartzJob).WithName("ResumeQuartzJob").WithTags("Quartz");

        return endpoints;
    }

    private static async Task<IResult> ListQuartzJobs(
        HttpContext httpContext,
        CancellationToken token)
    {
        var accessor = httpContext.RequestServices.GetRequiredService<IQuartzSchedulerAccessor>();
        var scheduler = accessor.Scheduler;
        if (scheduler is null)
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), token).ConfigureAwait(false);
        var summaries = new List<object>();
        foreach (var jobKey in jobKeys)
        {
            var triggers = await scheduler.GetTriggersOfJob(jobKey, token).ConfigureAwait(false);
            summaries.Add(new
            {
                job = new { jobKey.Name, jobKey.Group },
                triggers = triggers.Select(t => new
                {
                    trigger = new { t.Key.Name, t.Key.Group },
                    nextFireTimeUtc = t.GetNextFireTimeUtc()?.UtcDateTime,
                    previousFireTimeUtc = t.GetPreviousFireTimeUtc()?.UtcDateTime,
                    description = t.Description
                })
            });
        }
        return Results.Ok(summaries);
    }

    private static async Task<IResult> TriggerQuartzJob(
        string group,
        string name,
        HttpContext httpContext,
        CancellationToken token)
    {
        var accessor = httpContext.RequestServices.GetRequiredService<IQuartzSchedulerAccessor>();
        var scheduler = accessor.Scheduler;
        if (scheduler is null)
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

        var jobKey = new JobKey(name, group);
        await scheduler.TriggerJob(jobKey, token).ConfigureAwait(false);
        return Results.Accepted();
    }

    private static async Task<IResult> PauseQuartzJob(
        string group,
        string name,
        HttpContext httpContext,
        CancellationToken token)
    {
        var accessor = httpContext.RequestServices.GetRequiredService<IQuartzSchedulerAccessor>();
        var scheduler = accessor.Scheduler;
        if (scheduler is null)
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

        await scheduler.PauseJob(new JobKey(name, group), token).ConfigureAwait(false);
        return Results.Accepted();
    }

    private static async Task<IResult> ResumeQuartzJob(
        string group,
        string name,
        HttpContext httpContext,
        CancellationToken token)
    {
        var accessor = httpContext.RequestServices.GetRequiredService<IQuartzSchedulerAccessor>();
        var scheduler = accessor.Scheduler;
        if (scheduler is null)
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

        await scheduler.ResumeJob(new JobKey(name, group), token).ConfigureAwait(false);
        return Results.Accepted();
    }
}

