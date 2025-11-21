import React, { useState, useEffect } from 'react';
import { usersApi, rolesApi, type User, type Role } from '../api';
import './UsersPage.css';

export const UsersPage: React.FC = () => {
    const [users, setUsers] = useState<User[]>([]);
    const [roles, setRoles] = useState<Role[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [selectedUser, setSelectedUser] = useState<User | null>(null);
    const [userRoles, setUserRoles] = useState<Role[]>([]);
    const [resetPasswordResult, setResetPasswordResult] = useState<{ userId: string, username: string, password: string } | null>(null);
    const [confirmResetUser, setConfirmResetUser] = useState<User | null>(null);

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        setIsLoading(true);
        setError('');
        try {
            const [usersData, rolesData] = await Promise.all([
                usersApi.list(),
                rolesApi.list(),
            ]);
            setUsers(usersData);
            setRoles(rolesData);
        } catch (err: any) {
            setError(err.message || 'Failed to load data');
        } finally {
            setIsLoading(false);
        }
    };

    const handleViewUser = async (user: User) => {
        setSelectedUser(user);
        try {
            const roles = await usersApi.getRoles(user.id);
            setUserRoles(roles);
        } catch (err: any) {
            setError(err.message || 'Failed to load user roles');
        }
    };

    const handleAssignRole = async (roleId: string) => {
        if (!selectedUser) return;

        try {
            await usersApi.assignRole(selectedUser.id, { roleId });
            const updatedRoles = await usersApi.getRoles(selectedUser.id);
            setUserRoles(updatedRoles);
        } catch (err: any) {
            setError(err.message || 'Failed to assign role');
        }
    };

    const handleRevokeRole = async (roleId: string) => {
        if (!selectedUser) return;

        try {
            await usersApi.revokeRole(selectedUser.id, roleId);
            const updatedRoles = await usersApi.getRoles(selectedUser.id);
            setUserRoles(updatedRoles);
        } catch (err: any) {
            setError(err.message || 'Failed to revoke role');
        }
    };

    const handleResetPassword = async (user: User) => {
        setConfirmResetUser(user);
    };

    const confirmResetPassword = async () => {
        if (!confirmResetUser) return;

        try {
            const newPassword = await usersApi.resetPassword(confirmResetUser.id);
            setResetPasswordResult({ userId: confirmResetUser.id, username: confirmResetUser.username, password: newPassword });
            setConfirmResetUser(null);
        } catch (err: any) {
            setError(err.message || 'Failed to reset password');
            setConfirmResetUser(null);
        }
    };

    if (isLoading) {
        return (
            <div className="page-container">
                <div className="flex items-center justify-center" style={{ minHeight: '400px' }}>
                    <div className="spinner" style={{ width: '40px', height: '40px' }}></div>
                </div>
            </div>
        );
    }

    return (
        <div className="page-container">
            <div className="page-header">
                <div>
                    <h1>Users</h1>
                    <p className="text-muted">Manage user accounts and permissions</p>
                </div>
                <button className="btn btn-primary" onClick={() => setShowCreateModal(true)}>
                    Create User
                </button>
            </div>

            {error && (
                <div className="alert alert-error">
                    {error}
                </div>
            )}

            <div className="card">
                <table className="table">
                    <thead>
                        <tr>
                            <th>Username</th>
                            <th>Email</th>
                            <th>ID</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map((user) => (
                            <tr key={user.id}>
                                <td><strong>{user.username}</strong></td>
                                <td>{user.email}</td>
                                <td className="text-muted">{user.id.slice(0, 8)}...</td>
                                <td>
                                    <button
                                        className="btn btn-sm btn-secondary"
                                        onClick={() => handleViewUser(user)}
                                    >
                                        Manage Roles
                                    </button>
                                    <button
                                        className="btn btn-sm btn-secondary"
                                        onClick={() => handleResetPassword(user)}
                                        style={{ marginLeft: 'var(--spacing-sm)' }}
                                        title="Reset Password"
                                    >
                                        🔑 Reset
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>

                {users.length === 0 && (
                    <div className="empty-state">
                        <p className="text-muted">No users found</p>
                    </div>
                )}
            </div>

            {/* User Roles Modal */}
            {selectedUser && (
                <div className="modal-overlay" onClick={() => setSelectedUser(null)}>
                    <div className="modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2 className="modal-title">
                                Roles for {selectedUser.username}
                            </h2>
                            <button className="modal-close" onClick={() => setSelectedUser(null)}>
                                ×
                            </button>
                        </div>

                        <div className="modal-body">
                            <h3 style={{ marginBottom: 'var(--spacing-md)', fontSize: 'var(--font-size-sm)' }}>
                                Assigned Roles
                            </h3>
                            <div className="flex gap-sm" style={{ flexWrap: 'wrap', marginBottom: 'var(--spacing-lg)' }}>
                                {userRoles.map((role) => (
                                    <span key={role.id} className="badge badge-primary">
                                        {role.name}
                                        <button
                                            onClick={() => handleRevokeRole(role.id)}
                                            style={{
                                                marginLeft: 'var(--spacing-sm)',
                                                background: 'none',
                                                border: 'none',
                                                color: 'inherit',
                                                cursor: 'pointer',
                                                fontSize: 'var(--font-size-lg)'
                                            }}
                                        >
                                            ×
                                        </button>
                                    </span>
                                ))}
                                {userRoles.length === 0 && (
                                    <p className="text-muted">No roles assigned</p>
                                )}
                            </div>

                            <h3 style={{ marginBottom: 'var(--spacing-md)', fontSize: 'var(--font-size-sm)' }}>
                                Available Roles
                            </h3>
                            <div className="flex flex-col gap-sm">
                                {roles
                                    .filter((role) => !userRoles.some((ur) => ur.id === role.id))
                                    .map((role) => (
                                        <button
                                            key={role.id}
                                            className="btn btn-sm btn-secondary"
                                            onClick={() => handleAssignRole(role.id)}
                                            style={{ justifyContent: 'flex-start' }}
                                        >
                                            + {role.name}
                                        </button>
                                    ))}
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* Create User Modal - Placeholder */}
            {showCreateModal && (
                <div className="modal-overlay" onClick={() => setShowCreateModal(false)}>
                    <div className="modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2 className="modal-title">Create User</h2>
                            <button className="modal-close" onClick={() => setShowCreateModal(false)}>
                                ×
                            </button>
                        </div>
                        <div className="modal-body">
                            <p className="text-muted">
                                User creation coming soon! Currently, users can only register through the /auth/register endpoint.
                            </p>
                        </div>
                    </div>
                </div>
            )}

            {/* Password Reset Result Modal */}
            {resetPasswordResult && (
                <div className="modal-overlay" onClick={() => setResetPasswordResult(null)}>
                    <div className="modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2 className="modal-title">Password Reset Successfully</h2>
                            <button className="modal-close" onClick={() => setResetPasswordResult(null)}>
                                ×
                            </button>
                        </div>
                        <div className="modal-body">
                            <p>New password for <strong>{resetPasswordResult.username}</strong>:</p>
                            <div className="password-display">
                                <code>{resetPasswordResult.password}</code>
                                <button
                                    className="btn btn-sm btn-secondary"
                                    onClick={() => {
                                        navigator.clipboard.writeText(resetPasswordResult.password);
                                        alert('Password copied to clipboard!');
                                    }}
                                    style={{ marginLeft: 'var(--spacing-md)' }}
                                >
                                    📋 Copy
                                </button>
                            </div>
                            <p className="warning" style={{ marginTop: 'var(--spacing-md)', padding: 'var(--spacing-md)', background: 'var(--color-warning-bg)', border: '1px solid var(--color-warning)', borderRadius: 'var(--radius-md)' }}>
                                ⚠️ Save this password now. It cannot be retrieved later.
                            </p>
                            <button
                                className="btn btn-primary"
                                onClick={() => setResetPasswordResult(null)}
                                style={{ marginTop: 'var(--spacing-lg)', width: '100%' }}
                            >
                                Close
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Confirm Reset Password Modal */}
            {confirmResetUser && (
                <div className="modal-overlay" onClick={() => setConfirmResetUser(null)}>
                    <div className="modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2 className="modal-title">Confirm Password Reset</h2>
                            <button className="modal-close" onClick={() => setConfirmResetUser(null)}>
                                ×
                            </button>
                        </div>
                        <div className="modal-body">
                            <p>Generate a new password for <strong>{confirmResetUser.username}</strong>?</p>
                            <p className="text-muted" style={{ marginTop: 'var(--spacing-md)' }}>
                                They will need the new password to login.
                            </p>
                            <div style={{ display: 'flex', gap: 'var(--spacing-md)', marginTop: 'var(--spacing-lg)' }}>
                                <button
                                    className="btn btn-secondary"
                                    onClick={() => setConfirmResetUser(null)}
                                    style={{ flex: 1 }}
                                >
                                    Cancel
                                </button>
                                <button
                                    className="btn btn-primary"
                                    onClick={confirmResetPassword}
                                    style={{ flex: 1 }}
                                >
                                    Reset Password
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};
