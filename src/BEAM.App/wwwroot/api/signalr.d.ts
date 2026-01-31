import { MetricsSnapshot } from './metrics';
type MetricsCallback = (snapshot: MetricsSnapshot) => void;
declare class MetricsSignalR {
    private connection;
    private listeners;
    start(): Promise<void>;
    stop(): void;
    subscribe(callback: MetricsCallback): () => void;
}
export declare const metricsSignalR: MetricsSignalR;
export {};
