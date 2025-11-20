import axios from 'axios';
import type { Feature } from './types';
import apiClient from './client';

export const featuresApi = {
    async list(): Promise<Feature[]> {
        // Override baseURL so we call /api/features (client default is /auth)
        const response = await apiClient.get<Feature[]>('/api/features', { baseURL: '/' });
        return response.data;
    },

    async listDirect(): Promise<Feature[]> {
        // Fallback direct call if the apiClient base URL ever changes
        const response = await axios.get<Feature[]>('/api/features', {
            headers: buildAuthHeaders(),
            withCredentials: true
        });
        return response.data;
    }
};

function buildAuthHeaders() {
    const token = localStorage.getItem('accessToken');
    return token ? { Authorization: `Bearer ${token}` } : {};
}
