export interface LogEntry {
    sequence: number;
    timestampUtc: string;
    level: string;
    message: string;
    exception?: string | null;
    properties?: Record<string, string | null>;
}
export declare const logsApi: {
    listRecent(take?: number, since?: number): Promise<LogEntry[]>;
};
