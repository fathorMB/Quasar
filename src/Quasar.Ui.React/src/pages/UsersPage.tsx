import React, { useState, useEffect } from 'react';
import { usersApi, rolesApi, authApi, type User, type Role } from '../api';
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
    const [copied, setCopied] = useState(false);
    const [createForm, setCreateForm] = useState({ username: '', email: '', password: '', confirmPassword: '' });
    const [isCreating, setIsCreating] = useState(false);

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

    const handleCreateUser = async () => {
        if (createForm.password !== createForm.confirmPassword) {
            setError('Passwords do not match');
            return;
        }
        setIsCreating(true);
        setError('');
        try {
            await authApi.register({
                username: createForm.username.trim(),
                email: createForm.email.trim(),
                password: createForm.password,
            });
            setShowCreateModal(false);
            setCreateForm({ username: '', email: '', password: '', confirmPassword: '' });
            await loadData();
        } catch (err: any) {
            setError(err.message || 'Failed to create user');
        } finally {
            setIsCreating(false);
        }
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

            {/* Create User Modal */}
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
                            <div className="form-group">
                                <label className="label">Username</label>
                                <input
                                    className="input"
                                    value={createForm.username}
                                    onChange={(e) => setCreateForm({ ...createForm, username: e.target.value })}
                                    placeholder="Username"
                                />
                            </div>
                            <div className="form-group">
                                <label className="label">Email</label>
                                <input
                                    className="input"
                                    type="email"
                                    value={createForm.email}
                                    onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })}
                                    placeholder="user@example.com"
                                />
                            </div>
                            <div className="form-group">
                                <label className="label">Password</label>
                                <input
                                    className="input"
                                    type="password"
                                    value={createForm.password}
                                    onChange={(e) => setCreateForm({ ...createForm, password: e.target.value })}
                                    placeholder="Password"
                                />
                            </div>
                            <div className="form-group">
                                <label className="label">Confirm Password</label>
                                <input
                                    className="input"
                                    type="password"
                                    value={createForm.confirmPassword}
                                    onChange={(e) => setCreateForm({ ...createForm, confirmPassword: e.target.value })}
                                    placeholder="Confirm Password"
                                />
                            </div>
                            <div style={{ display: 'flex', gap: 'var(--spacing-md)', marginTop: 'var(--spacing-lg)' }}>
                                <button
                                    className="btn btn-secondary"
                                    onClick={() => setShowCreateModal(false)}
                                    disabled={isCreating}
                                    style={{ flex: 1 }}
                                >
                                    Cancel
                                </button>
                                <button
                                    className="btn btn-primary"
                                    onClick={handleCreateUser}
                                    disabled={isCreating || !createForm.username || !createForm.email || !createForm.password || !createForm.confirmPassword}
                                    style={{ flex: 1 }}
                                >
                                    {isCreating ? 'Creating...' : 'Create'}
                                </button>
                            </div>
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
                                        setCopied(true);
                                        setTimeout(() => setCopied(false), 2000);
                                    }}
                                    style={{ marginLeft: 'var(--spacing-md)' }}
                                >
                                    {copied ? '✓ Copied!' : '📋 Copy'}
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
