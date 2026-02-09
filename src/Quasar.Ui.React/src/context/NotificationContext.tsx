import React, { createContext, useContext, useState, type ReactNode } from 'react';

export interface Notification {
    id: string;
    title: string;
    message: string;
    type: 'info' | 'success' | 'warning' | 'error';
    timestamp: Date;
    read: boolean;
}

interface NotificationContextType {
    notifications: Notification[];
    unreadCount: number;
    addNotification: (notification: Omit<Notification, 'id' | 'timestamp' | 'read'>) => void;
    markAsRead: (id: string) => void;
    markAllAsRead: () => void;
    clearNotifications: () => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

import { useAuth } from './AuthContext';
import { notificationSignalR, fetchUnreadNotifications, markNotificationAsRead, markAllNotificationsAsRead } from '../api/notification';

export const NotificationProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [notifications, setNotifications] = useState<Notification[]>([]);
    const { isAuthenticated } = useAuth();

    const unreadCount = notifications.filter(n => !n.read).length;

    const addNotification = React.useCallback((n: Omit<Notification, 'id' | 'timestamp' | 'read'> & { id?: string, createdAt?: string }) => {
        const newNotification: Notification = {
            ...n,
            id: n.id || Math.random().toString(36).substring(2, 9),
            timestamp: n.createdAt ? new Date(n.createdAt) : new Date(),
            read: false,
        };
        setNotifications(prev => {
            const exists = prev.some(p => p.id === newNotification.id);
            if (exists) return prev;
            return [newNotification, ...prev].slice(0, 50);
        });
    }, []);

    // Load and Connect effect
    React.useEffect(() => {
        let mounted = true;

        const load = async () => {
            if (!isAuthenticated) {
                setNotifications([]);
                notificationSignalR.stop().catch(console.error);
                return;
            }

            // Restart SignalR with new token
            // Await stop to ensure clean state before starting new connection
            await notificationSignalR.stop();
            await notificationSignalR.start();

            // Fetch missed notifications
            const data = await fetchUnreadNotifications();
            if (mounted) {
                const mapped = data.map(n => ({
                    id: n.id,
                    title: n.title,
                    message: n.message,
                    type: n.type,
                    timestamp: new Date(n.createdAt),
                    read: n.isRead
                }));
                setNotifications(prev => {
                    const existingIds = new Set(prev.map(p => p.id));
                    const newItems = mapped.filter(m => !existingIds.has(m.id));
                    return [...newItems, ...prev].sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime());
                });
            }
        };

        load();

        const unsubscribe = notificationSignalR.subscribe((n) => {
            addNotification({
                id: n.id,
                title: n.title,
                message: n.message,
                type: n.type,
                createdAt: n.createdAt
            });
        });

        return () => {
            mounted = false;
            unsubscribe();
            notificationSignalR.stop().catch(console.error);
        };
    }, [isAuthenticated, addNotification]);


    const markAsRead = React.useCallback((id: string) => {
        markNotificationAsRead(id);
        setNotifications(prev => prev.map(n => n.id === id ? { ...n, read: true } : n));
    }, []);

    const markAllAsRead = React.useCallback(() => {
        markAllNotificationsAsRead();
        setNotifications(prev => prev.map(n => ({ ...n, read: true })));
    }, []);

    const clearNotifications = React.useCallback(() => {
        setNotifications([]);
    }, []);

    return (
        <NotificationContext.Provider value={{
            notifications,
            unreadCount,
            addNotification,
            markAsRead,
            markAllAsRead,
            clearNotifications
        }}>
            {children}
        </NotificationContext.Provider>
    );
};

export const useNotifications = () => {
    const context = useContext(NotificationContext);
    if (!context) {
        throw new Error('useNotifications must be used within a NotificationProvider');
    }
    return context;
};
