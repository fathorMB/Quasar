import { User, Role, AssignUserRoleRequest } from './types';
export declare const usersApi: {
    /**
     * Get all users
     */
    list: () => Promise<User[]>;
    /**
     * Get roles for a specific user
     */
    getRoles: (userId: string) => Promise<Role[]>;
    /**
     * Get permissions for a specific user
     */
    getPermissions: (userId: string) => Promise<string[]>;
    /**
     * Assign a role to a user
     */
    assignRole: (userId: string, data: AssignUserRoleRequest) => Promise<void>;
    /**
     * Revoke a role from a user
     */
    revokeRole: (userId: string, roleId: string) => Promise<void>;
    /**
     * Reset a user's password (admin only)
     * Returns the new generated password
     */
    resetPassword: (userId: string) => Promise<string>;
    /**
     * Reset own password (any authenticated user)
     * Returns the new generated password
     */
    resetOwnPassword: () => Promise<string>;
    delete: (userId: string) => Promise<void>;
};
