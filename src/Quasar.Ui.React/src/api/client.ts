import axios from 'axios';
import type { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios';
import type { ApiError } from './types';

// Create axios instance with base configuration
const apiClient: AxiosInstance = axios.create({
    baseURL: '/auth',
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true, // Important for cookie-based auth if needed
});

// Request interceptor to add auth token
apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('accessToken');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

        // If 401 and we have a refresh token, try to refresh
        if (error.response?.status === 401 && originalRequest) {
            const refreshToken = localStorage.getItem('refreshToken');

            if (refreshToken && !originalRequest._retry) {
                originalRequest._retry = true;

                try {
                    const response = await axios.post('/auth/token/refresh', {
                        refreshToken,
                    });

                    const { accessToken: newAccessToken, refreshToken: newRefreshToken } = response.data;

                    // Update tokens
                    localStorage.setItem('accessToken', newAccessToken);
                    localStorage.setItem('refreshToken', newRefreshToken);

                    // Retry original request with new token
                    originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
                    return apiClient(originalRequest);
                } catch (refreshError) {
                    // Refresh failed, clear tokens and redirect to login
                    localStorage.removeItem('accessToken');
                    localStorage.removeItem('refreshToken');
                    window.location.href = '/login';
                    return Promise.reject(refreshError);
                }
            }
        }

        // Transform error to ApiError format
        const isLoginRequest = error.config?.url?.endsWith('/login');
        const errorData = error.response?.data as any;

        // Check if this is a session revocation
        if (error.response?.status === 401 && errorData?.code === 'SESSION_REVOKED') {
            // Clear tokens
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');

            // Dispatch custom event to show modal
            window.dispatchEvent(new CustomEvent('session-revoked', {
                detail: { message: errorData.message }
            }));

            const apiError: ApiError = {
                message: errorData.message,
                statusCode: 401,
            };
            return Promise.reject(apiError);
        }

        const apiError: ApiError = {
            message: error.response?.status === 401 && isLoginRequest
                ? 'Invalid username or password'
                : (errorData?.message || error.message || 'An error occurred'),
            statusCode: error.response?.status || 500,
        };

        return Promise.reject(apiError);
    }
);

export default apiClient;
