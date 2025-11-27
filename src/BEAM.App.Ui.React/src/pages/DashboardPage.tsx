import React, { useEffect, useState } from 'react';
import { devicesApi } from '../api/devices';
import type { Device } from '../api/devices';

export const DashboardPage: React.FC = () => {
    const [devices, setDevices] = useState<Device[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadDevices();
        const id = setInterval(() => {
            loadDevices();
        }, 5000);
        return () => clearInterval(id);
    }, []);

    const loadDevices = async () => {
        try {
            setLoading(true);
            setError(null);
            const result = await devicesApi.list(1, 100);
            setDevices(result.items);
        } catch (err: any) {
            const errorMessage = err.response?.status === 404
                ? 'Device API not available. Make sure BEAM.App is running with the latest code.'
                : err.message || 'Failed to load devices. Please try again.';
            setError(errorMessage);
        } finally {
            setLoading(false);
        }
    };

    const formatDate = (dateString: string | null) => {
        if (!dateString) return 'Never';
        return new Date(dateString).toLocaleString();
    };

    return (
        <div className="page-container">
            <div className="page-header">
                <div>
                    <h1>Dashboard</h1>
                    <p className="text-muted">Welcome to BEAM Device Management</p>
                </div>
            </div>

            <div className="card">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 'var(--spacing-md)' }}>
                    <h2>Registered Devices</h2>
                    <button
                        className="btn btn-secondary btn-sm"
                        onClick={loadDevices}
                        disabled={loading}
                    >
                        {loading ? 'Refreshing...' : 'Refresh'}
                    </button>
                </div>

                {loading ? (
                    <p className="text-muted">Loading devices...</p>
                ) : error ? (
                    <div style={{ padding: 'var(--spacing-lg)', textAlign: 'center' }}>
                        <p style={{ color: 'var(--color-danger)', marginBottom: 'var(--spacing-sm)' }}>
                            {error}
                        </p>
                        <button className="btn btn-primary btn-sm" onClick={loadDevices}>
                            Retry
                        </button>
                    </div>
                ) : devices.length === 0 ? (
                    <div style={{ padding: 'var(--spacing-lg)', textAlign: 'center' }}>
                        <p className="text-muted">No devices registered yet</p>
                        <p style={{ fontSize: '0.875rem', color: 'var(--color-text-secondary)' }}>
                            Use the BEAM.Emulator to register your first device
                        </p>
                    </div>
                ) : (
                    <div className="table-container">
                        <table>
                            <thead>
                                <tr>
                                    <th style={{ width: '26%' }}>Device</th>
                                    <th style={{ width: '15%' }}>Type</th>
                                    <th style={{ width: '18%' }}>MAC</th>
                                    <th style={{ width: '18%' }}>Status</th>
                                    <th style={{ width: '12%' }}>Registered</th>
                                    <th style={{ width: '11%' }}>Last Seen</th>
                                </tr>
                            </thead>
                            <tbody>
                                {devices.map((device) => (
                                    <tr key={device.id}>
                                        <td>
                                            <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                                                <div style={{ fontWeight: 700 }}>{device.deviceName}</div>
                                                <div style={{ fontSize: '0.75rem', color: 'var(--color-text-secondary)', wordBreak: 'break-all' }}>
                                                    {device.id}
                                                </div>
                                            </div>
                                        </td>
                                        <td>
                                            <code style={{ fontWeight: 600 }}>{device.deviceType}</code>
                                        </td>
                                        <td>
                                            <code style={{ letterSpacing: '0.03em' }}>{device.macAddress}</code>
                                        </td>
                                        <td>
                                            <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap' }}>
                                                <span
                                                    className="badge"
                                                    style={{
                                                        background: device.isActive ? 'var(--color-success)' : 'var(--color-border)',
                                                        color: device.isActive ? '#06120a' : 'var(--color-text-secondary)',
                                                        fontWeight: 700,
                                                    }}
                                                >
                                                    {device.isActive ? 'Active' : 'Inactive'}
                                                </span>
                                                <span
                                                    className="badge"
                                                    style={{
                                                        background: device.isConnected ? 'var(--color-primary)' : 'rgba(255,255,255,0.08)',
                                                        color: device.isConnected ? '#0c0b0b' : 'var(--color-text-secondary)',
                                                        fontWeight: 700,
                                                    }}
                                                >
                                                    {device.isConnected ? 'Connected' : 'Offline'}
                                                </span>
                                            </div>
                                        </td>
                                        <td style={{ whiteSpace: 'nowrap' }}>{formatDate(device.registeredAt)}</td>
                                        <td style={{ whiteSpace: 'nowrap' }}>{formatDate(device.lastSeenAt || null)}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}

                {!loading && devices.length > 0 && (
                    <div style={{ marginTop: 'var(--spacing-md)', paddingTop: 'var(--spacing-md)', borderTop: '1px solid var(--color-border)', color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>
                        Total devices: {devices.length}
                    </div>
                )}
            </div>
        </div>
    );
};
