import apiClient from './client';

export interface QuartzJob {
    job: {
        name: string;
        group: string;
    };
    triggers: {
        trigger: {
            name: string;
            group: string;
        };
        nextFireTimeUtc?: string;
        previousFireTimeUtc?: string;
        description?: string;
    }[];
}

export interface QuartzHistoryRecord {
    job: { name: string; group: string };
    trigger: { name: string; group: string };
    scheduledFireTimeUtc?: string | null;
    fireTimeUtc: string;
    endTimeUtc?: string | null;
    nextFireTimeUtc?: string | null;
    success: boolean;
    error?: string | null;
}

export const quartzApi = {
    async listJobs(): Promise<QuartzJob[]> {
        const response = await apiClient.get<QuartzJob[]>('/quartz/jobs', { baseURL: '/' });
        return response.data;
    },

    async triggerJob(group: string, name: string): Promise<void> {
        await apiClient.post(`/quartz/jobs/${group}/${name}/trigger`, {}, { baseURL: '/' });
    },

    async pauseJob(group: string, name: string): Promise<void> {
        await apiClient.post(`/quartz/jobs/${group}/${name}/pause`, {}, { baseURL: '/' });
    },

    async resumeJob(group: string, name: string): Promise<void> {
        await apiClient.post(`/quartz/jobs/${group}/${name}/resume`, {}, { baseURL: '/' });
    },

    async listHistory(take = 50): Promise<QuartzHistoryRecord[]> {
        const response = await apiClient.get<QuartzHistoryRecord[]>('/quartz/history', {
            baseURL: '/',
            params: { take }
        });
        return response.data;
    }
};
