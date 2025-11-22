import * as signalR from '@microsoft/signalr';
import type { MetricsSnapshot } from './metrics';

type MetricsCallback = (snapshot: MetricsSnapshot) => void;

class MetricsSignalR {
    private connection: signalR.HubConnection | null = null;
    private listeners: MetricsCallback[] = [];

    public async start() {
        if (this.connection) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/metrics')
            .withAutomaticReconnect()
            .build();

        this.connection.on('ReceiveSnapshot', (snapshot: MetricsSnapshot) => {
            this.listeners.forEach(listener => listener(snapshot));
        });

        try {
            await this.connection.start();
            console.log('SignalR Connected');
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
            this.connection = null;
        }
    }

    public stop() {
        if (this.connection) {
            this.connection.stop();
            this.connection = null;
        }
    }

    public subscribe(callback: MetricsCallback) {
        this.listeners.push(callback);
        return () => {
            this.listeners = this.listeners.filter(l => l !== callback);
        };
    }
}

export const metricsSignalR = new MetricsSignalR();
