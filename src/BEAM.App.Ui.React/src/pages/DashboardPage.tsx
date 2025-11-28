import React, { useEffect, useState, useCallback, useRef } from 'react';
import { devicesApi } from '../api/devices';
import { useLiveReadModelCollection } from '../hooks/useLiveReadModel';
import type { Device } from '../api/devices';

export const DashboardPage: React.FC = () => {
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const initialLoadedRef = useRef(false);

    // Set up real-time updates via live projections
    const { models: devices, addOrUpdate } = useLiveReadModelCollection<Device>(
        'DeviceReadModel'
    );

    // Load initial devices on mount
    const loadDevices = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);
            const result = await devicesApi.list(1, 100);
            // Populate the live collection with initial data
            result.items.forEach(device => {
                addOrUpdate(device);
            });
        } catch (err: any) {
            const errorMessage = err.response?.status === 404
                ? 'Device API not available. Make sure BEAM.App is running with the latest code.'
                : err.message || 'Failed to load devices. Please try again.';
            setError(errorMessage);
        } finally {
            setLoading(false);
        }
    }, [addOrUpdate]);

    // Initial load on mount - only once
    useEffect(() => {
        if (!initialLoadedRef.current) {
            initialLoadedRef.current = true;
            loadDevices();
        }
    }, [loadDevices]);

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
                                    <th style={{ width: '24%' }}>Device</th>
                                    <th style={{ width: '13%' }}>Type</th>
                                    <th style={{ width: '16%' }}>MAC</th>
                                    <th style={{ width: '16%' }}>Status</th>
                                    <th style={{ width: '11%' }}>Registered</th>
                                    <th style={{ width: '11%' }}>Last Seen</th>
                                    <th style={{ width: '9%' }}>Actions</th>
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
                                                        background: device.isConnected ? '#10b981' : '#ef4444',
                                                        color: '#ffffff',
                                                        fontWeight: 700,
                                                    }}
                                                >
                                                    {device.isConnected ? '🟢 Connected' : '🔴 Offline'}
                                                </span>
                                            </div>
                                        </td>
                                        <td style={{ whiteSpace: 'nowrap' }}>{formatDate(device.registeredAt)}</td>
                                        <td style={{ whiteSpace: 'nowrap' }}>{formatDate(device.lastSeenAt || null)}</td>
                                        <td>
                                            <button
                                                className="btn btn-primary btn-sm"
                                                onClick={(e) => {
                                                    e.preventDefault();
                                                    const url = `/device/${device.id}`;
                                                    window.history.pushState({}, '', url);
                                                    // Trigger popstate to notify router
                                                    window.dispatchEvent(new PopStateEvent('popstate'));
                                                }}
                                                style={{ fontSize: '0.875rem' }}
                                            >
                                                Configure
                                            </button>
                                        </td>
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
