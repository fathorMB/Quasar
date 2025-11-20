import apiClient from './client';
import type {
    LoginRequest,
    RegisterRequest,
    RefreshTokenRequest,
    LogoutRequest,
    AuthTokens,
    RegisterResponse,
} from './types';

export const authApi = {
    /**
     * Login with username and password
     */
    login: async (credentials: LoginRequest): Promise<AuthTokens> => {
        const response = await apiClient.post<AuthTokens>('/login', credentials);

        // Store tokens in localStorage
        localStorage.setItem('accessToken', response.data.accessToken);
        localStorage.setItem('refreshToken', response.data.refreshToken);

        return response.data;
    },

    /**
     * Register a new user
     */
    register: async (data: RegisterRequest): Promise<RegisterResponse> => {
        const response = await apiClient.post<RegisterResponse>('/register', data);
        return response.data;
    },

    /**
     * Refresh access token
     */
    refreshToken: async (data: RefreshTokenRequest): Promise<AuthTokens> => {
        const response = await apiClient.post<AuthTokens>('/token/refresh', data);

        // Update tokens in localStorage
        localStorage.setItem('accessToken', response.data.accessToken);
        localStorage.setItem('refreshToken', response.data.refreshToken);

        return response.data;
    },

    /**
     * Logout and revoke refresh token
     */
    logout: async (): Promise<void> => {
        const refreshToken = localStorage.getItem('refreshToken');

        if (refreshToken) {
            try {
                await apiClient.post('/logout', { refreshToken } as LogoutRequest);
            } catch (error) {
                // Ignore logout errors
                console.error('Logout error:', error);
            }
        }

        // Clear tokens from localStorage
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
    },

    /**
     * Check if user is authenticated
     */
    isAuthenticated: (): boolean => {
        return !!localStorage.getItem('accessToken');
    },
};
