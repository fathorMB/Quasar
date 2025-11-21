using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Quasar.Scheduling.Quartz;

/// <summary>
/// Thread-safe in-memory store for recent job execution records.
/// </summary>
public sealed class JobExecutionHistoryStore
{
    private readonly ConcurrentQueue<JobExecutionRecord> _records = new();
    private readonly int _capacity;

    public JobExecutionHistoryStore(int capacity = 200)
    {
        _capacity = capacity;
    }

    public void Add(JobExecutionRecord record)
    {
        _records.Enqueue(record);
        while (_records.Count > _capacity && _records.TryDequeue(out _)) { }
    }

    public IReadOnlyList<JobExecutionRecord> GetRecent(int take = 50)
    {
        return _records
            .OrderByDescending(r => r.FireTimeUtc)
            .Take(Math.Max(1, take))
            .ToList();
    }
}
