import * as signalR from '@microsoft/signalr';

export interface SignalRNotification {
    title: string;
    message: string;
    type: 'info' | 'success' | 'warning' | 'error';
}

type NotificationCallback = (notification: SignalRNotification) => void;

class NotificationSignalR {
    private connection: signalR.HubConnection | null = null;
    private listeners: NotificationCallback[] = [];

    public async start() {
        if (this.connection) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/notifications')
            .withAutomaticReconnect()
            .build();

        this.connection.on('ReceiveNotification', (notification: SignalRNotification) => {
            this.listeners.forEach(listener => listener(notification));
        });

        try {
            await this.connection.start();
            console.log('Notification SignalR Connected');
        } catch (err) {
            console.error('Notification SignalR Connection Error: ', err);
            this.connection = null;
        }
    }

    public stop() {
        if (this.connection) {
            this.connection.stop();
            this.connection = null;
        }
    }

    public subscribe(callback: NotificationCallback) {
        this.listeners.push(callback);
        return () => {
            this.listeners = this.listeners.filter(l => l !== callback);
        };
    }
}

export const notificationSignalR = new NotificationSignalR();
