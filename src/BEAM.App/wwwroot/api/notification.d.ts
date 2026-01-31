export interface SignalRNotification {
    title: string;
    message: string;
    type: 'info' | 'success' | 'warning' | 'error';
}
type NotificationCallback = (notification: SignalRNotification) => void;
declare class NotificationSignalR {
    private connection;
    private listeners;
    start(): Promise<void>;
    stop(): void;
    subscribe(callback: NotificationCallback): () => void;
}
export declare const notificationSignalR: NotificationSignalR;
export {};
