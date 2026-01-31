import { Session } from './types';
export declare const sessionsApi: {
    /**
     * List all active sessions (Admin only)
     */
    getAll: () => Promise<Session[]>;
    /**
     * Revoke a specific session (Admin only)
     */
    revoke: (sessionId: string) => Promise<void>;
};
