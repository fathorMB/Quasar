using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Serilog.Events;

namespace Quasar.Logging;

/// <summary>
/// In-memory buffer of recent log entries used to surface diagnostics through APIs or UI.
/// </summary>
public sealed class InMemoryLogBuffer
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();
    private int _capacity;
    private long _sequence;

    public InMemoryLogBuffer(int capacity = 512)
    {
        Resize(capacity);
    }

    /// <summary>
    /// Gets the maximum number of log entries retained in memory.
    /// </summary>
    public int Capacity => _capacity;

    /// <summary>
    /// Changes the capacity of the in-memory buffer.
    /// </summary>
    public void Resize(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        Interlocked.Exchange(ref _capacity, capacity);
        Trim();
    }

    /// <summary>
    /// Adds a Serilog <see cref="LogEvent"/> to the buffer.
    /// </summary>
    public void Store(LogEvent logEvent)
    {
        if (logEvent is null) throw new ArgumentNullException(nameof(logEvent));

        var sequence = Interlocked.Increment(ref _sequence);
        var properties = logEvent.Properties.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value?.ToString(),
            StringComparer.OrdinalIgnoreCase);

        var entry = new LogEntry(
            Sequence: sequence,
            TimestampUtc: logEvent.Timestamp.UtcDateTime,
            Level: logEvent.Level.ToString(),
            Message: logEvent.RenderMessage(),
            Exception: logEvent.Exception?.ToString(),
            Properties: properties);

        _entries.Enqueue(entry);
        Trim();
    }

    /// <summary>
    /// Retrieves log entries captured after the supplied sequence number.
    /// </summary>
    public IReadOnlyList<LogEntry> GetEntries(long? afterSequence)
    {
        var snapshot = _entries.ToArray();
        if (afterSequence is null or <= 0)
        {
            return snapshot;
        }
        return snapshot.Where(e => e.Sequence > afterSequence).ToArray();
    }

    private void Trim()
    {
        while (_entries.Count > _capacity && _entries.TryDequeue(out _)) { }
    }
}

/// <summary>
/// Lightweight DTO describing a captured log event.
/// </summary>
public sealed record LogEntry(
    long Sequence,
    DateTime TimestampUtc,
    string Level,
    string Message,
    string? Exception,
    IReadOnlyDictionary<string, string?> Properties);

