import apiClient from './client';
import type { User, Role, AssignUserRoleRequest } from './types';

export const usersApi = {
    /**
     * Get all users
     */
    list: async (): Promise<User[]> => {
        const response = await apiClient.get<User[]>('/users');
        return response.data;
    },

    /**
     * Get roles for a specific user
     */
    getRoles: async (userId: string): Promise<Role[]> => {
        const response = await apiClient.get<Role[]>(`/users/${userId}/roles`);
        return response.data;
    },

    /**
     * Get permissions for a specific user
     */
    getPermissions: async (userId: string): Promise<string[]> => {
        const response = await apiClient.get<string[]>(`/users/${userId}/permissions`);
        return response.data;
    },

    /**
     * Assign a role to a user
     */
    assignRole: async (userId: string, data: AssignUserRoleRequest): Promise<void> => {
        await apiClient.post(`/users/${userId}/roles`, data);
    },

    /**
     * Revoke a role from a user
     */
    revokeRole: async (userId: string, roleId: string): Promise<void> => {
        await apiClient.delete(`/users/${userId}/roles/${roleId}`);
    },

    /**
     * Reset a user's password (admin only)
     * Returns the new generated password
     */
    resetPassword: async (userId: string): Promise<string> => {
        const response = await apiClient.post<{ password: string }>(`/users/${userId}/reset-password`);
        return response.data.password;
    },

    /**
     * Reset own password (any authenticated user)
     * Returns the new generated password
     */
    resetOwnPassword: async (): Promise<string> => {
        const response = await apiClient.post<{ password: string }>('/users/me/reset-password');
        return response.data.password;
    },
};
