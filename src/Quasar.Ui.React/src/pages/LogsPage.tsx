import React, { useEffect, useState } from 'react';
import { logsApi } from '../api';
import type { LogEntry } from '../api/logs';
import './LogsPage.css';

const levelClass = (level: string) => {
    const l = level.toLowerCase();
    if (l.includes('error') || l === 'fatal') return 'badge-error';
    if (l.includes('warn')) return 'badge-warning';
    if (l.includes('debug') || l.includes('trace')) return 'badge-secondary';
    return 'badge-success';
};

export const LogsPage: React.FC = () => {
    const [logs, setLogs] = useState<LogEntry[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');

    const loadLogs = async () => {
        setIsLoading(true);
        setError('');
        try {
            const data = await logsApi.listRecent(200);
            setLogs(data);
        } catch (err: any) {
            setError(err.message || 'Failed to load logs');
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        loadLogs();
    }, []);

    return (
        <div className="page-container">
            <div className="page-header">
                <div>
                    <h1>Logs</h1>
                    <p className="text-muted">Recent server log entries</p>
                </div>
                <button className="btn btn-primary" onClick={loadLogs} disabled={isLoading}>
                    {isLoading ? 'Refreshing...' : 'Refresh'}
                </button>
            </div>

            {error && (
                <div className="alert alert-error">
                    {error}
                </div>
            )}

            <div className="card">
                <table className="table logs-table">
                    <thead>
                        <tr>
                            <th>Time</th>
                            <th>Level</th>
                            <th>Message</th>
                            <th>Exception</th>
                        </tr>
                    </thead>
                    <tbody>
                        {logs.map((log) => (
                            <tr key={log.sequence}>
                                <td className="text-muted">{new Date(log.timestampUtc).toLocaleString()}</td>
                                <td>
                                    <span className={`badge ${levelClass(log.level)}`}>{log.level}</span>
                                </td>
                                <td>{log.message}</td>
                                <td className="text-muted" style={{ maxWidth: '360px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                    {log.exception || ''}
                                </td>
                            </tr>
                        ))}
                        {logs.length === 0 && (
                            <tr>
                                <td colSpan={4} className="text-center text-muted">No log entries</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
};
