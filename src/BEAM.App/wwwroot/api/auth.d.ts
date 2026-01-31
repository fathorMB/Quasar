import { LoginRequest, RegisterRequest, RefreshTokenRequest, AuthTokens, RegisterResponse } from './types';
export declare const authApi: {
    /**
     * Login with username and password
     */
    login: (credentials: LoginRequest) => Promise<AuthTokens>;
    /**
     * Register a new user
     */
    register: (data: RegisterRequest) => Promise<RegisterResponse>;
    /**
     * Refresh access token
     */
    refreshToken: (data: RefreshTokenRequest) => Promise<AuthTokens>;
    /**
     * Logout and revoke refresh token
     */
    logout: () => Promise<void>;
    /**
     * Check if user is authenticated
     */
    isAuthenticated: () => boolean;
};
