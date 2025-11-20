import React from 'react';

export const DashboardPage: React.FC = () => {
    return (
        <div className="page-container">
            <div className="page-header">
                <div>
                    <h1>Dashboard</h1>
                    <p className="text-muted">Welcome to BEAM Identity Server</p>
                </div>
            </div>

            <div className="card card-gradient">
                <h2 style={{ marginBottom: 'var(--spacing-md)' }}>Getting Started</h2>
                <p style={{ marginBottom: 'var(--spacing-md)', color: 'var(--color-text-secondary)' }}>
                    This is the BEAM identity server administration interface. Use the navigation to manage users and roles.
                </p>
                <div className="flex gap-md">
                    <a href="/users" className="btn btn-primary">
                        Manage Users
                    </a>
                    <a href="/roles" className="btn btn-secondary">
                        Manage Roles
                    </a>
                </div>
            </div>
        </div>
    );
};
