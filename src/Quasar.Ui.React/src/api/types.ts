// ============================================
// Request Types
// ============================================

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

// ============================================
// Response Types
// ============================================

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
}

export interface Role {
  id: string;
  name: string;
}

export interface UserRole {
  userId: string;
  username: string;
  roleId: string;
  roleName: string;
}

export interface RegisterResponse {
  userId: string;
}

export interface CreateRoleResponse {
  roleId: string;
}

export interface Feature {
  id: string;
  name: string;
  category: string;
  description: string;
  status: string;
  details?: string | null;
}

// ============================================
// Error Types
// ============================================

export interface ApiError {
  message: string;
  statusCode: number;
}
