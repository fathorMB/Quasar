import { Role, CreateRoleRequest, CreateRoleResponse, GrantPermissionRequest, DeleteRoleResponse } from './types';
export declare const rolesApi: {
    /**
     * Get all roles
     */
    list: () => Promise<Role[]>;
    /**
     * Create a new role
     */
    create: (data: CreateRoleRequest) => Promise<CreateRoleResponse>;
    /**
     * Get permissions for a specific role
     */
    getPermissions: (roleId: string) => Promise<string[]>;
    /**
     * Grant a permission to a role
     */
    grantPermission: (roleId: string, data: GrantPermissionRequest) => Promise<void>;
    /**
     * Revoke a permission from a role
     */
    revokePermission: (roleId: string, permission: string) => Promise<void>;
    delete: (roleId: string) => Promise<DeleteRoleResponse>;
};
