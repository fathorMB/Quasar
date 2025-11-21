import apiClient from './client';

export interface LogEntry {
    sequence: number;
    timestampUtc: string;
    level: string;
    message: string;
    exception?: string | null;
    properties?: Record<string, string | null>;
}

export const logsApi = {
    async listRecent(take = 200, since?: number): Promise<LogEntry[]> {
        const response = await apiClient.get<LogEntry[]>('/logs/recent', {
            baseURL: '/',
            params: { take, since }
        });
        return response.data;
    }
};
