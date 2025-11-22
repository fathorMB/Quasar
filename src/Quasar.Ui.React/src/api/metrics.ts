import apiClient from './client';

export interface MetricsSnapshot {
    totalRequests: number;
    requestsLastMinute: number;
    requestsLastHour: number;
    averageLatencyMs: number;
    p95LatencyMs: number;
    p99LatencyMs: number;
    statusCodeCounts: Record<number, number>;
    topEndpoints: EndpointMetric[];
    recentExceptions: ExceptionEntry[];
    uptime: string;
    cpuUsagePercent: number;
    memoryUsageBytes: number;
}

export interface EndpointMetric {
    path: string;
    count: number;
    avgLatencyMs: number;
}

export interface ExceptionEntry {
    timestamp: string;
    type: string;
    message: string;
    endpoint?: string;
}

export const metricsApi = {
    async getSnapshot(): Promise<MetricsSnapshot> {
        // Override baseURL because metrics endpoint is at root /api/metrics, not under /auth
        const response = await apiClient.get<MetricsSnapshot>('/api/metrics', { baseURL: '/' });
        return response.data;
    }
};
