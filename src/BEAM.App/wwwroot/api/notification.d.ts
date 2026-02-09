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
export interface ApiNotification {
    id: string;
    userId: string;
    title: string;
    message: string;
    type: 'info' | 'success' | 'warning' | 'error';
    isRead: boolean;
    createdAt: string;
}
export declare const fetchUnreadNotifications: () => Promise<ApiNotification[]>;
export declare const markNotificationAsRead: (id: string) => Promise<void>;
export declare const markAllNotificationsAsRead: () => Promise<void>;
export {};
