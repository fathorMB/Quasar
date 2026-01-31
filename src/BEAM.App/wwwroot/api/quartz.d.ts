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
    job: {
        name: string;
        group: string;
    };
    trigger: {
        name: string;
        group: string;
    };
    scheduledFireTimeUtc?: string | null;
    fireTimeUtc: string;
    endTimeUtc?: string | null;
    nextFireTimeUtc?: string | null;
    success: boolean;
    error?: string | null;
}
export declare const quartzApi: {
    listJobs(): Promise<QuartzJob[]>;
    triggerJob(group: string, name: string): Promise<void>;
    pauseJob(group: string, name: string): Promise<void>;
    resumeJob(group: string, name: string): Promise<void>;
    listHistory(take?: number): Promise<QuartzHistoryRecord[]>;
};
