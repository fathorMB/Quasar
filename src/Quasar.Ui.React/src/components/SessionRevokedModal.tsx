import React from 'react';
import '../styles/components.css';

interface SessionRevokedModalProps {
    onClose: () => void;
}

export const SessionRevokedModal: React.FC<SessionRevokedModalProps> = ({ onClose }) => {
    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                <div className="modal-header">
                    <h2>Session Revoked</h2>
                </div>
                <div className="modal-body">
                    <div className="alert alert-warning">
                        <svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor" style={{ marginRight: '8px' }}>
                            <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                        </svg>
                        <span>Your session has been revoked. This may have happened because:</span>
                    </div>
                    <ul style={{ marginLeft: '32px', marginTop: '12px', marginBottom: '16px' }}>
                        <li>An administrator revoked your session</li>
                        <li>You logged in from another device</li>
                        <li>Your session expired</li>
                    </ul>
                    <p style={{ color: 'var(--color-text-secondary)' }}>
                        You will be redirected to the login page.
                    </p>
                </div>
                <div className="modal-footer">
                    <button className="btn btn-primary" onClick={onClose}>
                        Go to Login
                    </button>
                </div>
            </div>
        </div>
    );
};
