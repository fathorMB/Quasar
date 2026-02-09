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
    private recentIds = new Set<string>();

    public async start() {
        if (this.connection) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/notifications', {
                accessTokenFactory: () => localStorage.getItem('accessToken') || ''
            })
            .withAutomaticReconnect()
            .build();

        this.connection.on('ReceiveNotification', (raw: any) => {
            // Normalize casing: SignalR sends PascalCase, we want camelCase
            const notification: SignalRNotification = {
                id: raw.id || raw.Id || '',
                title: raw.title || raw.Title || '',
                message: raw.message || raw.Message || '',
                type: (raw.type || raw.Type || 'info').toLowerCase(),
                createdAt: raw.createdAt || raw.CreatedAt || new Date().toISOString(),
            };

            // Dedup: skip if we already delivered this notification ID recently
            if (notification.id && this.recentIds.has(notification.id)) {
                return;
            }
            if (notification.id) {
                this.recentIds.add(notification.id);
                // Clean up after 10 seconds
                setTimeout(() => this.recentIds.delete(notification.id), 10000);
            }

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

    public async stop() {
        if (this.connection) {
            try {
                this.connection.off('ReceiveNotification');
                await this.connection.stop();
            } catch (err) {
                console.error('Error stopping SignalR connection:', err);
            } finally {
                this.connection = null;
            }
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
