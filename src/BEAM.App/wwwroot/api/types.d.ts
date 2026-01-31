export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
}
export interface LoginRequest {
    username: string;
    password: string;
}
export interface RefreshTokenRequest {
    refreshToken: string;
}
export interface LogoutRequest {
    refreshToken: string;
}
export interface CreateRoleRequest {
    name: string;
}
export interface GrantPermissionRequest {
    permission: string;
}
export interface AssignUserRoleRequest {
    roleId: string;
}
export interface AuthTokens {
    accessToken: string;
    accessExpiresUtc: string;
    refreshToken: string;
    refreshExpiresUtc: string;
}
export interface User {
    id: string;
    username: string;
    email: string;
    roles?: string[];
    isDeleted?: boolean;
    deletedAtUtc?: string | null;
}
export interface Role {
    id: string;
    name: string;
    isDeleted?: boolean;
    deletedAtUtc?: string | null;
}
export interface RegisterResponse {
    userId: string;
}
export interface CreateRoleResponse {
    roleId: string;
}
export interface DeleteRoleResponse {
    success: boolean;
    message?: string;
}
export interface Feature {
    id: string;
    name: string;
    category: string;
    description: string;
    status: string;
    details?: string | null;
}
export interface ApiError {
    message: string;
    statusCode: number;
}
export interface Session {
    sessionId: string;
    userId: string;
    username: string;
    issuedUtc: string;
    expiresUtc: string;
    revokedUtc: string | null;
    isActive: boolean;
}
