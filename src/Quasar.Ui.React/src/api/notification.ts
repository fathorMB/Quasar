import * as signalR from '@microsoft/signalr';

export interface SignalRNotification {
    id: string;
    title: string;
    message: string;
    type: 'info' | 'success' | 'warning' | 'error';
    createdAt: string;
}

type NotificationCallback = (notification: SignalRNotification) => void;

class NotificationSignalR {
    private connection: signalR.HubConnection | null = null;
    private listeners: NotificationCallback[] = [];

    public async start() {
        if (this.connection) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/notifications', {
                accessTokenFactory: () => localStorage.getItem('accessToken') || ''
            })
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

export interface ApiNotification {
    id: string;
    userId: string;
    title: string;
    message: string;
    type: 'info' | 'success' | 'warning' | 'error';
    isRead: boolean;
    createdAt: string;
}

export const fetchUnreadNotifications = async (): Promise<ApiNotification[]> => {
    const token = localStorage.getItem('accessToken');
    if (!token) return [];

    try {
        const response = await fetch('/api/player/notifications', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.status === 401) return [];
        if (!response.ok) throw new Error('Failed to fetch notifications');

        return await response.json();
    } catch (error) {
        console.error('Error fetching notifications:', error);
        return [];
    }
};

export const markNotificationAsRead = async (id: string): Promise<void> => {
    const token = localStorage.getItem('accessToken');
    if (!token) return;

    try {
        await fetch(`/api/player/notifications/${id}/read`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
    } catch (error) {
        console.error('Error marking notification as read:', error);
    }
};

export const markAllNotificationsAsRead = async (): Promise<void> => {
    const token = localStorage.getItem('accessToken');
    if (!token) return;

    try {
        await fetch('/api/player/notifications/read-all', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
    } catch (error) {
        console.error('Error marking all notifications as read:', error);
    }
};
