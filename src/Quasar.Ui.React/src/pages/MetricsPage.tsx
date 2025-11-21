import React, { useEffect, useState } from 'react';
import { metricsApi, type MetricsSnapshot } from '../api/metrics';
import { useFeatures } from '../context/FeatureContext';
import './MetricsPage.css';

export const MetricsPage: React.FC = () => {
    const [metrics, setMetrics] = useState<MetricsSnapshot | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const { hasFeature } = useFeatures();

    const fetchMetrics = async () => {
        try {
            setError(null);
            const data = await metricsApi.getSnapshot();
            console.log('API Response:', data);
            setMetrics(data);
        } catch (err) {
            console.error('Failed to fetch metrics:', err);
            setError(err instanceof Error ? err.message : 'Failed to load metrics');
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        if (!hasFeature('telemetry')) {
            setIsLoading(false);
            return;
        }

        fetchMetrics();
        const interval = setInterval(fetchMetrics, 5000);
        return () => clearInterval(interval);
    }, [hasFeature]);

    if (!hasFeature('telemetry')) {
        return (
            <div className="container">
                <h1>Telemetry & Metrics</h1>
                <div className="alert alert-warning">
                    Telemetry feature is not enabled.
                </div>
            </div>
        );
    }

    if (isLoading) {
        return <div className="container">Loading metrics...</div>;
    }

    if (error) {
        return (
            <div className="container">
                <h1>Telemetry & Metrics</h1>
                <div className="alert alert-danger">
                    {error}
                </div>
                <button className="btn btn-primary" onClick={fetchMetrics}>
                    Retry
                </button>
            </div>
        );
    }

    if (!metrics) {
        return <div className="container">No metrics data available</div>;
    }

    // Safe defaults for all properties
    const totalRequests = metrics.totalRequests ?? 0;
    const requestsLastMinute = metrics.requestsLastMinute ?? 0;
    const requestsLastHour = metrics.requestsLastHour ?? 0;
    const averageLatencyMs = metrics.averageLatencyMs ?? 0;
    const p95LatencyMs = metrics.p95LatencyMs ?? 0;
    const p99LatencyMs = metrics.p99LatencyMs ?? 0;
    const uptime = metrics.uptime ?? '0.00:00:00';
    const statusCodeCounts = metrics.statusCodeCounts ?? {};
    const topEndpoints = metrics.topEndpoints ?? [];
    const recentExceptions = metrics.recentExceptions ?? [];

    const errorRate = totalRequests > 0
        ? ((statusCodeCounts[500] || 0) / totalRequests * 100).toFixed(2)
        : '0.00';

    return (
        <div className="container">
            <div className="header-actions">
                <h1>Telemetry & Metrics</h1>
                <button className="btn btn-secondary" onClick={fetchMetrics}>
                    Refresh
                </button>
            </div>

            <div className="metrics-grid">
                <div className="metric-card">
                    <h3>Total Requests</h3>
                    <div className="metric-value">{totalRequests.toLocaleString()}</div>
                    <div className="metric-subtitle">
                        Last minute: {requestsLastMinute} | Last hour: {requestsLastHour}
                    </div>
                </div>

                <div className="metric-card">
                    <h3>Average Latency</h3>
                    <div className="metric-value">{averageLatencyMs.toFixed(2)} ms</div>
                    <div className="metric-subtitle">
                        P95: {p95LatencyMs.toFixed(2)} ms | P99: {p99LatencyMs.toFixed(2)} ms
                    </div>
                </div>

                <div className="metric-card">
                    <h3>Error Rate</h3>
                    <div className="metric-value">{errorRate}%</div>
                    <div className="metric-subtitle">
                        5xx errors: {statusCodeCounts[500] || 0}
                    </div>
                </div>

                <div className="metric-card">
                    <h3>Uptime</h3>
                    <div className="metric-value">{formatUptime(uptime)}</div>
                </div>
            </div>

            {topEndpoints.length > 0 && (
                <div className="card">
                    <h2>Top Endpoints</h2>
                    <table className="table">
                        <thead>
                            <tr>
                                <th>Path</th>
                                <th>Requests</th>
                                <th>Avg Latency</th>
                            </tr>
                        </thead>
                        <tbody>
                            {topEndpoints.map((ep, idx) => (
                                <tr key={idx}>
                                    <td><code>{ep.path}</code></td>
                                    <td>{ep.count.toLocaleString()}</td>
                                    <td>{ep.avgLatencyMs.toFixed(2)} ms</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {recentExceptions.length > 0 && (
                <div className="card">
                    <h2>Recent Exceptions</h2>
                    <table className="table">
                        <thead>
                            <tr>
                                <th>Time</th>
                                <th>Type</th>
                                <th>Message</th>
                                <th>Endpoint</th>
                            </tr>
                        </thead>
                        <tbody>
                            {recentExceptions.map((ex, idx) => (
                                <tr key={idx}>
                                    <td>{new Date(ex.timestamp).toLocaleString()}</td>
                                    <td><code>{ex.type}</code></td>
                                    <td>{ex.message}</td>
                                    <td>{ex.endpoint || '-'}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            <div className="card">
                <h2>Status Code Distribution</h2>
                {Object.keys(statusCodeCounts).length > 0 ? (
                    <div className="status-bars">
                        {Object.entries(statusCodeCounts)
                            .sort(([a], [b]) => Number(a) - Number(b))
                            .map(([code, count]) => {
                                const percentage = ((count as number) / totalRequests * 100).toFixed(1);
                                return (
                                    <div key={code} className="status-bar-row">
                                        <span className="status-code">{code}</span>
                                        <div className="status-bar">
                                            <div
                                                className={`status-bar-fill status-${code[0]}xx`}
                                                style={{ width: `${percentage}%` }}
                                            />
                                        </div>
                                        <span className="status-count">{count as number} ({percentage}%)</span>
                                    </div>
                                );
                            })}
                    </div>
                ) : (
                    <p>No requests recorded yet.</p>
                )}
            </div>
        </div>
    );
};

function formatUptime(uptime: string): string {
    const match = uptime.match(/(\d+)\.(\d+):(\d+):(\d+)/);
    if (!match) return uptime;
    const [, days, hours, minutes] = match;
    return `${days}d ${hours}h ${minutes}m`;
}
