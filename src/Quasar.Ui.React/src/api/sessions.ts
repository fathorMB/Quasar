import apiClient from './client';
import type { Session } from './types';

export const sessionsApi = {
    /**
     * List all active sessions (Admin only)
     */
    getAll: async (): Promise<Session[]> => {
        const response = await apiClient.get<Session[]>('/sessions');
        return response.data;
    },

    /**
     * Revoke a specific session (Admin only)
     */
    revoke: async (sessionId: string): Promise<void> => {
        await apiClient.delete(`/sessions/${sessionId}`);
    }
};
