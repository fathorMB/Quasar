import axios from 'axios';
import type { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios';

const apiClient: AxiosInstance = axios.create({
    baseURL: '/auth',
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true,
});

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

apiClient.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

        if (error.response?.status === 401 && originalRequest) {
            const refreshToken = localStorage.getItem('refreshToken');

            if (refreshToken && !originalRequest._retry) {
                originalRequest._retry = true;

                try {
                    const response = await axios.post('/auth/token/refresh', {
                        refreshToken,
                    });

                    const { accessToken: newAccessToken, refreshToken: newRefreshToken } = response.data;

                    localStorage.setItem('accessToken', newAccessToken);
                    localStorage.setItem('refreshToken', newRefreshToken);

                    originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
                    return apiClient(originalRequest);
                } catch (refreshError) {
                    localStorage.removeItem('accessToken');
                    localStorage.removeItem('refreshToken');
                    window.location.href = '/login';
                    return Promise.reject(refreshError);
                }
            }
        }

        const isLoginRequest = error.config?.url?.endsWith('/login');
        const errorData = error.response?.data as any;

        if (error.response?.status === 401 && errorData?.code === 'SESSION_REVOKED') {
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');

            window.dispatchEvent(new CustomEvent('session-revoked', {
                detail: { message: errorData.message }
            }));

            return Promise.reject({
                message: errorData.message,
                statusCode: 401,
            });
        }

        return Promise.reject({
            message: error.response?.status === 401 && isLoginRequest
                ? 'Invalid username or password'
                : (errorData?.message || error.message || 'An error occurred'),
            statusCode: error.response?.status || 500,
        });
    }
);

export default apiClient;
