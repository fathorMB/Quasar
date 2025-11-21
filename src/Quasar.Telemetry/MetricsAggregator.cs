using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Quasar.Telemetry;

public sealed record MetricsSnapshot(
    long TotalRequests,
    long RequestsLastMinute,
    long RequestsLastHour,
    double AverageLatencyMs,
    double P95LatencyMs,
    double P99LatencyMs,
    Dictionary<int, long> StatusCodeCounts,
    List<EndpointMetric> TopEndpoints,
    List<ExceptionEntry> RecentExceptions,
    TimeSpan Uptime);

public sealed record EndpointMetric(string Path, long Count, double AvgLatencyMs);

public sealed record ExceptionEntry(DateTimeOffset Timestamp, string Type, string Message, string? Endpoint);

public interface IMetricsAggregator
{
    MetricsSnapshot GetSnapshot();
    void RecordRequest(string endpoint, int statusCode, double latencyMs);
    void RecordException(Exception ex, string? endpoint);
}

public sealed class InMemoryMetricsAggregator : IMetricsAggregator
{
    private readonly DateTimeOffset _startTime = DateTimeOffset.UtcNow;
    private readonly ConcurrentDictionary<string, EndpointStats> _endpoints = new();
    private readonly ConcurrentDictionary<int, long> _statusCodes = new();
    private readonly ConcurrentQueue<RequestRecord> _recentRequests = new();
    private readonly ConcurrentQueue<ExceptionEntry> _recentExceptions = new();
    private readonly object _lock = new();
    
    private long _totalRequests;
    private const int MaxRecentRequests = 10000;
    private const int MaxRecentExceptions = 50;
    private const int TopEndpointsCount = 10;

    public void RecordRequest(string endpoint, int statusCode, double latencyMs)
    {
        Interlocked.Increment(ref _totalRequests);
        
        // Update status code counts
        _statusCodes.AddOrUpdate(statusCode, 1, (_, count) => count + 1);
        
        // Update endpoint stats
        _endpoints.AddOrUpdate(endpoint,
            _ => new EndpointStats(endpoint, 1, latencyMs, latencyMs),
            (_, existing) =>
            {
                var newCount = existing.Count + 1;
                var newTotal = existing.TotalLatency + latencyMs;
                return existing with 
                { 
                    Count = newCount,
                    TotalLatency = newTotal,
                    LatencySum = newTotal
                };
            });
        
        // Track recent requests for time-based stats
        var record = new RequestRecord(DateTimeOffset.UtcNow, endpoint, statusCode, latencyMs);
        _recentRequests.Enqueue(record);
        
        // Trim old records
        while (_recentRequests.Count > MaxRecentRequests)
        {
            _recentRequests.TryDequeue(out _);
        }
    }

    public void RecordException(Exception ex, string? endpoint)
    {
        var entry = new ExceptionEntry(
            DateTimeOffset.UtcNow,
            ex.GetType().Name,
            ex.Message,
            endpoint);
        
        _recentExceptions.Enqueue(entry);
        
        while (_recentExceptions.Count > MaxRecentExceptions)
        {
            _recentExceptions.TryDequeue(out _);
        }
    }

    public MetricsSnapshot GetSnapshot()
    {
        var now = DateTimeOffset.UtcNow;
        var oneMinuteAgo = now.AddMinutes(-1);
        var oneHourAgo = now.AddHours(-1);
        
        // Filter recent requests by time
        var recentRequestsList = _recentRequests.ToList();
        var lastMinute = recentRequestsList.Count(r => r.Timestamp >= oneMinuteAgo);
        var lastHour = recentRequestsList.Count(r => r.Timestamp >= oneHourAgo);
        
        // Calculate latency percentiles from recent requests
        var latencies = recentRequestsList
            .Select(r => r.LatencyMs)
            .OrderBy(l => l)
            .ToList();
        
        var avgLatency = latencies.Any() ? latencies.Average() : 0;
        var p95Latency = GetPercentile(latencies, 95);
        var p99Latency = GetPercentile(latencies, 99);
        
        // Get top endpoints
        var topEndpoints = _endpoints.Values
            .OrderByDescending(e => e.Count)
            .Take(TopEndpointsCount)
            .Select(e => new EndpointMetric(
                e.Path,
                e.Count,
                e.Count > 0 ? e.TotalLatency / e.Count : 0))
            .ToList();
        
        // Copy status codes and exceptions
        var statusCodeCounts = new Dictionary<int, long>(_statusCodes);
        var exceptions = _recentExceptions.ToList();
        
        var uptime = now - _startTime;
        
        return new MetricsSnapshot(
            _totalRequests,
            lastMinute,
            lastHour,
            avgLatency,
            p95Latency,
            p99Latency,
            statusCodeCounts,
            topEndpoints,
            exceptions,
            uptime);
    }
    
    private static double GetPercentile(List<double> sortedValues, int percentile)
    {
        if (sortedValues.Count == 0) return 0;
        
        var index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;
        index = Math.Max(0, Math.Min(sortedValues.Count - 1, index));
        
        return sortedValues[index];
    }

    private sealed record EndpointStats(string Path, long Count, double TotalLatency, double LatencySum);
    private sealed record RequestRecord(DateTimeOffset Timestamp, string Endpoint, int StatusCode, double LatencyMs);
}
