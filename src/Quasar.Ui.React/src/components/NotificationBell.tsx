import React, { useState, useRef, useEffect } from 'react';
import { useNotifications } from '../context/NotificationContext';
import { notificationSignalR } from '../api/notification';
import './NotificationBell.css';

export const NotificationBell: React.FC = () => {
    const { notifications, unreadCount, addNotification, markAsRead, markAllAsRead } = useNotifications();
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    // Subscribe to real-time events
    useEffect(() => {
        const unsubscribe = notificationSignalR.subscribe((n) => {
            addNotification({
                title: n.title,
                message: n.message,
                type: n.type
            });
        });

        notificationSignalR.start();

        return () => {
            unsubscribe();
            // We usually don't stop the connection globally if other components use it,
            // but for now this is fine or we keep it alive for the session.
        };
    }, [addNotification]);

    // Close on click outside
    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        };

        if (isOpen) {
            document.addEventListener('mousedown', handleClickOutside);
        }
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, [isOpen]);

    const formatTime = (date: Date) => {
        const now = new Date();
        const diff = now.getTime() - date.getTime();
        const minutes = Math.floor(diff / 60000);
        if (minutes < 1) return 'Just now';
        if (minutes < 60) return `${minutes}m ago`;
        const hours = Math.floor(minutes / 60);
        if (hours < 24) return `${hours}h ago`;
        return date.toLocaleDateString();
    };

    const handleNotificationClick = (id: string) => {
        markAsRead(id);
        // Additional logic like navigation could go here
    };

    return (
        <div className="notifications-container" ref={dropdownRef}>
            <button
                className="bell-button"
                onClick={() => setIsOpen(!isOpen)}
                aria-label="Notifications"
            >
                <span>🔔</span>
                {unreadCount > 0 && (
                    <span className="notification-badge">
                        {unreadCount > 9 ? '9+' : unreadCount}
                    </span>
                )}
            </button>

            {isOpen && (
                <div className="notifications-dropdown">
                    <div className="notifications-header">
                        <h3>Notifications</h3>
                        {unreadCount > 0 && (
                            <button className="mark-all-btn" onClick={markAllAsRead}>
                                Mark all as read
                            </button>
                        )}
                    </div>

                    <div className="notifications-list">
                        {notifications.length === 0 ? (
                            <div className="empty-notifications">
                                No notifications yet
                            </div>
                        ) : (
                            notifications.map(n => (
                                <div
                                    key={n.id}
                                    className={`notification-item ${n.read ? 'read' : 'unread'}`}
                                    onClick={() => handleNotificationClick(n.id)}
                                >
                                    <div className="notification-item-header">
                                        <span className={`notification-title type-${n.type}`}>
                                            {n.title}
                                        </span>
                                        <span className="notification-time">
                                            {formatTime(n.timestamp)}
                                        </span>
                                    </div>
                                    <div className="notification-message">
                                        {n.message}
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            )}
        </div>
    );
};
