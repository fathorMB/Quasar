import apiClient from './client';
import type { Role, CreateRoleRequest, CreateRoleResponse, GrantPermissionRequest, DeleteRoleResponse } from './types';

export const rolesApi = {
    /**
     * Get all roles
     */
    list: async (): Promise<Role[]> => {
        const response = await apiClient.get<Role[]>('/roles');
        return response.data;
    },

    /**
     * Create a new role
     */
    create: async (data: CreateRoleRequest): Promise<CreateRoleResponse> => {
        const response = await apiClient.post<CreateRoleResponse>('/roles', data);
        return response.data;
    },

    /**
     * Get permissions for a specific role
     */
    getPermissions: async (roleId: string): Promise<string[]> => {
        const response = await apiClient.get<string[]>(`/roles/${roleId}/permissions`);
        return response.data;
    },

    /**
     * Grant a permission to a role
     */
    grantPermission: async (roleId: string, data: GrantPermissionRequest): Promise<void> => {
        await apiClient.post(`/roles/${roleId}/permissions`, data);
    },

    /**
     * Revoke a permission from a role
     */
    revokePermission: async (roleId: string, permission: string): Promise<void> => {
        await apiClient.delete(`/roles/${roleId}/permissions/${encodeURIComponent(permission)}`);
    },

    delete: async (roleId: string): Promise<DeleteRoleResponse> => {
        const response = await apiClient.delete<DeleteRoleResponse>(`/roles/${roleId}`);
        return response.data;
    },
};
