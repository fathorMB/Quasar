import React, { useEffect, useState } from 'react';
import { sessionsApi } from '../api/sessions';
import type { Session } from '../api/types';
import { ConfirmModal } from '../components/ConfirmModal';
import './SessionsPage.css';

export const SessionsPage: React.FC = () => {
    const [sessions, setSessions] = useState<Session[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [confirmRevoke, setConfirmRevoke] = useState<{ show: boolean; sessionId: string | null }>({
        show: false,
        sessionId: null
    });

    const loadSessions = async () => {
        try {
            setIsLoading(true);
            const data = await sessionsApi.getAll();
            setSessions(data);
            setError(null);
        } catch (err) {
            console.error('Failed to load sessions:', err);
            setError('Failed to load sessions. You may not have permission.');
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        loadSessions();
    }, []);

    const handleRevokeClick = (sessionId: string) => {
        setConfirmRevoke({ show: true, sessionId });
    };

    const handleConfirmRevoke = async () => {
        if (!confirmRevoke.sessionId) return;

        try {
            await sessionsApi.revoke(confirmRevoke.sessionId);
            setConfirmRevoke({ show: false, sessionId: null });
            await loadSessions(); // Reload to show updated status
        } catch (err) {
            console.error('Failed to revoke session:', err);
            setError('Failed to revoke session.');
            setConfirmRevoke({ show: false, sessionId: null });
        }
    };

    if (isLoading) return <div className="container">Loading sessions...</div>;

    return (
        <div className="container">
            <div className="header-actions">
                <h1>Active Sessions</h1>
                <button className="btn btn-secondary" onClick={loadSessions}>
                    Refresh
                </button>
            </div>

            {error && <div className="alert alert-danger">{error}</div>}

            <div className="card">
                <table className="table">
                    <thead>
                        <tr>
                            <th>User</th>
                            <th>Issued</th>
                            <th>Expires</th>
                            <th>Status</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {sessions.map(session => (
                            <tr key={session.sessionId}>
                                <td>{session.username}</td>
                                <td>{new Date(session.issuedUtc).toLocaleString()}</td>
                                <td>{new Date(session.expiresUtc).toLocaleString()}</td>
                                <td>
                                    {session.isActive ? (
                                        <span className="badge badge-success">Active</span>
                                    ) : (
                                        <span className="badge badge-danger">Revoked</span>
                                    )}
                                </td>
                                <td>
                                    {session.isActive && (
                                        <button
                                            className="btn btn-danger btn-sm"
                                            onClick={() => handleRevokeClick(session.sessionId)}
                                        >
                                            Revoke
                                        </button>
                                    )}
                                </td>
                            </tr>
                        ))}
                        {sessions.length === 0 && (
                            <tr>
                                <td colSpan={5} className="text-center">No active sessions found.</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>

            {confirmRevoke.show && (
                <ConfirmModal
                    title="Revoke Session"
                    message="Are you sure you want to revoke this session? The user will be logged out immediately."
                    confirmText="Revoke"
                    cancelText="Cancel"
                    type="danger"
                    onConfirm={handleConfirmRevoke}
                    onCancel={() => setConfirmRevoke({ show: false, sessionId: null })}
                />
            )}
        </div>
    );
};
