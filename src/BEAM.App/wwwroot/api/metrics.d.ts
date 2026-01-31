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
    managedMemoryBytes: number;
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
export declare const metricsApi: {
    getSnapshot(): Promise<MetricsSnapshot>;
};
