using System;

namespace Quasar.Scheduling.Quartz;

/// <summary>
/// Captures a single execution of a Quartz job.
/// </summary>
public sealed record JobExecutionRecord(
    string JobName,
    string JobGroup,
    string? TriggerName,
    string? TriggerGroup,
    DateTimeOffset? ScheduledFireTimeUtc,
    DateTimeOffset FireTimeUtc,
    DateTimeOffset? NextFireTimeUtc,
    DateTimeOffset? EndTimeUtc,
    bool Success,
    string? Error);
