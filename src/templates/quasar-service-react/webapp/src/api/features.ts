import axios from 'axios';
import type { Feature } from './types';
import apiClient from './client';

export const featuresApi = {
    async list(): Promise<Feature[]> {
        const response = await apiClient.get<Feature[]>('/api/features', { baseURL: '/' });
        return response.data;
    },

    async listDirect(): Promise<Feature[]> {
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
