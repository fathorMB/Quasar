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
    }
};
