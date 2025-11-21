import React, { useState, useEffect } from 'react';
import { rolesApi, type Role } from '../api';

export const RolesPage: React.FC = () => {
    const [roles, setRoles] = useState<Role[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');
    const [selectedRole, setSelectedRole] = useState<Role | null>(null);
    const [rolePermissions, setRolePermissions] = useState<string[]>([]);
    const [newPermission, setNewPermission] = useState('');
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [createForm, setCreateForm] = useState({ name: '' });
    const [isCreating, setIsCreating] = useState(false);
    const [confirmDeleteRole, setConfirmDeleteRole] = useState<Role | null>(null);

    useEffect(() => {
        loadRoles();
    }, []);

    const loadRoles = async () => {
        setIsLoading(true);
        setError('');
        try {
            const data = await rolesApi.list();
            setRoles(data);
        } catch (err: any) {
            setError(err.message || 'Failed to load roles');
        } finally {
            setIsLoading(false);
        }
    };

    const handleViewRole = async (role: Role) => {
        setSelectedRole(role);
        try {
            const permissions = await rolesApi.getPermissions(role.id);
            setRolePermissions(permissions);
        } catch (err: any) {
            setError(err.message || 'Failed to load permissions');
        }
    };

    const handleGrantPermission = async () => {
        if (!selectedRole || !newPermission.trim()) return;

        try {
            await rolesApi.grantPermission(selectedRole.id, { permission: newPermission.trim() });
            const updatedPermissions = await rolesApi.getPermissions(selectedRole.id);
            setRolePermissions(updatedPermissions);
            setNewPermission('');
        } catch (err: any) {
            setError(err.message || 'Failed to grant permission');
        }
    };

    const handleRevokePermission = async (permission: string) => {
        if (!selectedRole) return;

        try {
            await rolesApi.revokePermission(selectedRole.id, permission);
            const updatedPermissions = await rolesApi.getPermissions(selectedRole.id);
            setRolePermissions(updatedPermissions);
        } catch (err: any) {
            setError(err.message || 'Failed to revoke permission');
        }
    };

    const handleCreateRole = async () => {
        if (!createForm.name.trim()) return;
        setIsCreating(true);
        setError('');
        try {
            await rolesApi.create({ name: createForm.name.trim() });
            setShowCreateModal(false);
            setCreateForm({ name: '' });
            await loadRoles();
        } catch (err: any) {
            setError(err.message || 'Failed to create role');
        } finally {
            setIsCreating(false);
        }
    };

    const handleDeleteRole = async () => {
        if (!confirmDeleteRole) return;
        setError('');
        try {
            const result = await rolesApi.delete(confirmDeleteRole.id);
            if (result && result.success === false) {
                setError(result.message || 'Role is assigned to users. Remove assignments before deleting.');
                return;
            }
            setConfirmDeleteRole(null);
            await loadRoles();
        } catch (err: any) {
            setError(err.message || 'Failed to delete role');
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
                    <h1>Roles</h1>
                    <p className="text-muted">Manage roles and permissions</p>
                </div>
                <button className="btn btn-primary" onClick={() => setShowCreateModal(true)}>
                    Create Role
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
                            <th>Role Name</th>
                            <th>ID</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {roles.map((role) => (
                            <tr key={role.id}>
                                <td>
                                    <strong>{role.name}</strong>
                                    {role.isDeleted && (
                                        <span className="badge badge-error" style={{ marginLeft: 'var(--spacing-sm)' }}>Deleted</span>
                                    )}
                                </td>
                                <td className="text-muted">{role.id.slice(0, 8)}...</td>
                                <td>
                                    <button
                                        className="btn btn-sm btn-secondary"
                                        onClick={() => handleViewRole(role)}
                                        disabled={!!role.isDeleted}
                                    >
                                        Manage Permissions
                                    </button>
                                    <button
                                        className="btn btn-sm btn-secondary"
                                        onClick={() => setConfirmDeleteRole(role)}
                                        style={{ marginLeft: 'var(--spacing-sm)' }}
                                        disabled={!!role.isDeleted}
                                    >
                                        Delete
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>

                {roles.length === 0 && (
                    <div className="empty-state">
                        <p className="text-muted">No roles found</p>
                    </div>
                )}
            </div>

            {/* Role Permissions Modal */}
            {selectedRole && (
                <div className="modal-overlay" onClick={() => setSelectedRole(null)}>
                    <div className="modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2 className="modal-title">
                                Permissions for {selectedRole.name}
                            </h2>
                            <button className="modal-close" onClick={() => setSelectedRole(null)}>
                                ×
                            </button>
                        </div>

                        <div className="modal-body">
                            <h3 style={{ marginBottom: 'var(--spacing-md)', fontSize: 'var(--font-size-sm)' }}>
                                Assigned Permissions
                            </h3>
                            <div className="flex gap-sm" style={{ flexWrap: 'wrap', marginBottom: 'var(--spacing-lg)' }}>
                                {rolePermissions.map((permission) => (
                                    <span key={permission} className="badge badge-primary">
                                        {permission}
                                        <button
                                            onClick={() => handleRevokePermission(permission)}
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
                                {rolePermissions.length === 0 && (
                                    <p className="text-muted">No permissions assigned</p>
                                )}
                            </div>

                            <h3 style={{ marginBottom: 'var(--spacing-md)', fontSize: 'var(--font-size-sm)' }}>
                                Grant Permission
                            </h3>
                            <div className="flex gap-sm">
                                <input
                                    type="text"
                                    className="input"
                                    placeholder="permission.name"
                                    value={newPermission}
                                    onChange={(e) => setNewPermission(e.target.value)}
                                    onKeyPress={(e) => e.key === 'Enter' && handleGrantPermission()}
                                />
                                <button
                                    className="btn btn-primary"
                                    onClick={handleGrantPermission}
                                    disabled={!newPermission.trim()}
                                >
                                    Grant
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* Create Role Modal */}
            {showCreateModal && (
                <div className="modal-overlay" onClick={() => setShowCreateModal(false)}>
                    <div className="modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2 className="modal-title">Create Role</h2>
                            <button className="modal-close" onClick={() => setShowCreateModal(false)}>
                                ×
                            </button>
                        </div>
                        <div className="modal-body">
                            <div className="form-group">
                                <label className="label">Role Name</label>
                                <input
                                    className="input"
                                    value={createForm.name}
                                    onChange={(e) => setCreateForm({ name: e.target.value })}
                                    placeholder="Role name"
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
                                    onClick={handleCreateRole}
                                    disabled={isCreating || !createForm.name.trim()}
                                    style={{ flex: 1 }}
                                >
                                    {isCreating ? 'Creating...' : 'Create'}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* Confirm Delete Role Modal */}
            {confirmDeleteRole && (
                <div className="modal-overlay" onClick={() => setConfirmDeleteRole(null)}>
                    <div className="modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2 className="modal-title">Delete Role</h2>
                            <button className="modal-close" onClick={() => setConfirmDeleteRole(null)}>
                                ×
                            </button>
                        </div>
                        <div className="modal-body">
                            <p>Are you sure you want to delete <strong>{confirmDeleteRole.name}</strong>?</p>
                            <p className="text-muted" style={{ marginTop: 'var(--spacing-md)' }}>
                                Roles assigned to users cannot be deleted.
                            </p>
                            <div style={{ display: 'flex', gap: 'var(--spacing-md)', marginTop: 'var(--spacing-lg)' }}>
                                <button className="btn btn-secondary" onClick={() => setConfirmDeleteRole(null)} style={{ flex: 1 }}>
                                    Cancel
                                </button>
                                <button className="btn btn-primary" onClick={handleDeleteRole} style={{ flex: 1 }}>
                                    Delete
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};
