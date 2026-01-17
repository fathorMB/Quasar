import React, { useState, useRef, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useUi } from '../context/UiContext';
import { usersApi } from '../api';
import './Header.css';

export const Header: React.FC = () => {
    const { user, logout } = useAuth();
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const [newPassword, setNewPassword] = useState<string | null>(null);
    const [showConfirmChange, setShowConfirmChange] = useState(false);
    const [copied, setCopied] = useState(false);
    const navigate = useNavigate();
    const menuRef = useRef<HTMLDivElement>(null);

    const handleLogout = async () => {
        await logout();
        navigate('/login');
    };

    const handleChangePassword = async () => {
        setShowConfirmChange(true);
        setIsMenuOpen(false);
    };

    const confirmChangePassword = async () => {
        try {
            const password = await usersApi.resetOwnPassword();
            setNewPassword(password);
            setShowConfirmChange(false);
        } catch (err: any) {
            console.error('Failed to change password:', err);
            alert('Failed to change password');
            setShowConfirmChange(false);
        }
    };

    // Close menu when clicking outside
    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
                setIsMenuOpen(false);
            }
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, []);

    const { pathname } = useLocation();
    const { customMenu } = useUi();

    const getPageTitle = () => {
        // Check custom menu first (with direct window fallback)
        const menuItems = customMenu.length > 0
            ? customMenu
            : ((window as any).__QUASAR_CUSTOM_MENU__ || []);

        const customItem = (menuItems as any[])
            .flatMap((section: any) => section.items)
            .find((item: any) => item.path === pathname);

        if (customItem) return customItem.label;

        // Fallback/Default routes
        switch (pathname) {
            case '/': return 'Dashboard'; // Fallback if not overridden
            case '/users': return 'Users';
            case '/roles': return 'Roles';
            case '/features': return 'Features';
            case '/jobs': return 'Jobs';
            case '/logs': return 'Logs';
            case '/metrics': return 'Metrics';
            case '/sessions': return 'Sessions';
            default: return 'Dashboard';
        }
    };

    const title = getPageTitle();

    return (
        <>
            <header className="app-header">
                <div className="header-left">
                    {/* Placeholder for page title or breadcrumbs if needed later */}
                    <h2 className="page-title">{title}</h2>
                </div>

                <div className="header-right">
                    <div className="user-profile">
                        <div className="user-info">
                            <span className="user-name">{user?.username || 'Guest'}</span>
                            <span className="user-role">{user?.roles?.[0] || 'User'}</span>
                        </div>
                        <div className="user-avatar">
                            {user?.username?.charAt(0).toUpperCase() || 'U'}
                        </div>
                    </div>
                    <div className="menu-container" ref={menuRef}>
                        <button
                            className="menu-button"
                            aria-label="Menu"
                            onClick={() => setIsMenuOpen(!isMenuOpen)}
                        >
                            <span className="menu-icon">⋮</span>
                        </button>

                        {isMenuOpen && (
                            <div className="dropdown-menu">
                                <button onClick={handleChangePassword} className="dropdown-item">
                                    Change Password
                                </button>
                                <button onClick={handleLogout} className="dropdown-item danger">
                                    Sign Out
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            </header>

            {/* Confirm Change Password Modal */}
            {showConfirmChange && (
                <div className="modal-overlay" onClick={() => setShowConfirmChange(false)}>
                    <div className="modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2 className="modal-title">Confirm Password Change</h2>
                            <button className="modal-close" onClick={() => setShowConfirmChange(false)}>
                                ×
                            </button>
                        </div>
                        <div className="modal-body">
                            <p>Generate a new password?</p>
                            <p className="text-muted" style={{ marginTop: 'var(--spacing-md)' }}>
                                You will be logged out and need to login with the new password.
                            </p>
                            <div style={{ display: 'flex', gap: 'var(--spacing-md)', marginTop: 'var(--spacing-lg)' }}>
                                <button
                                    className="btn btn-secondary"
                                    onClick={() => setShowConfirmChange(false)}
                                    style={{ flex: 1 }}
                                >
                                    Cancel
                                </button>
                                <button
                                    className="btn btn-primary"
                                    onClick={confirmChangePassword}
                                    style={{ flex: 1 }}
                                >
                                    Change Password
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* Password Change Result Modal */}
            {newPassword && (
                <div className="modal-overlay" onClick={() => {
                    setNewPassword(null);
                    handleLogout();
                }}>
                    <div className="modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2 className="modal-title">Password Changed Successfully</h2>
                        </div>
                        <div className="modal-body">
                            <p>Your new password is:</p>
                            <div className="password-display" style={{ display: 'flex', alignItems: 'center', gap: 'var(--spacing-md)', padding: 'var(--spacing-md)', background: 'var(--color-bg-secondary)', borderRadius: 'var(--radius-md)' }}>
                                <code style={{ flex: 1, fontSize: 'var(--font-size-base)', fontWeight: 'bold' }}>{newPassword}</code>
                                <button
                                    className="btn btn-sm btn-secondary"
                                    onClick={() => {
                                        navigator.clipboard.writeText(newPassword);
                                        setCopied(true);
                                        setTimeout(() => setCopied(false), 2000);
                                    }}
                                >
                                    {copied ? '✓ Copied!' : '📋 Copy'}
                                </button>
                            </div>
                            <p className="warning" style={{ marginTop: 'var(--spacing-md)', padding: 'var(--spacing-md)', background: 'var(--color-warning-bg)', border: '1px solid var(--color-warning)', borderRadius: 'var(--radius-md)' }}>
                                ⚠️ Save this password now. You will be logged out.
                            </p>
                            <button
                                className="btn btn-primary"
                                onClick={() => {
                                    setNewPassword(null);
                                    handleLogout();
                                }}
                                style={{ marginTop: 'var(--spacing-lg)', width: '100%' }}
                            >
                                Logout Now
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
};
