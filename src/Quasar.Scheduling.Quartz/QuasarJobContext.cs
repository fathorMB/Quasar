using System;
using System.Collections.Generic;

namespace Quasar.Scheduling.Quartz;

/// <summary>
/// Immutable data describing the current scheduled job invocation.
/// </summary>
public sealed record QuasarJobContext(
    string JobName,
    string JobGroup,
    string TriggerName,
    string TriggerGroup,
    string FireInstanceId,
    DateTime? ScheduledFireTimeUtc,
    DateTime FireTimeUtc,
    DateTime? PreviousFireTimeUtc,
    DateTime? NextFireTimeUtc,
    IReadOnlyDictionary<string, object?> Data);
