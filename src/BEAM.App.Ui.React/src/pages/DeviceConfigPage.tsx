import React, { useEffect, useState, useCallback, useRef } from 'react';
import { devicesApi } from '../api/devices';
import { useLiveReadModelCollection } from '../hooks/useLiveReadModel';
import { DevicePicker } from '../shared/DevicePicker';
import type { Device } from '../api/devices';

export const DeviceConfigPage: React.FC = () => {
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [selectedDeviceId, setSelectedDeviceId] = useState<string | null>(null);
    const [device, setDevice] = useState<Device | null>(null);

    // Form state
    const [deviceName, setDeviceName] = useState('');
    const [heartbeatInterval, setHeartbeatInterval] = useState<string | number>(60);
    const [saving, setSaving] = useState<'name' | 'interval' | null>(null);
    const [saveError, setSaveError] = useState<string | null>(null);
    const [saveSuccess, setSaveSuccess] = useState<string | null>(null);

    const initialLoadedRef = useRef(false);

    // Set up real-time updates via live projections
    const { models: devices, addOrUpdate } = useLiveReadModelCollection<Device>(
        'DeviceReadModel'
    );

    // Extract device ID from URL path (e.g. /device/123)
    useEffect(() => {
        const path = window.location.pathname;
        const match = path.match(/\/device\/([^\/]+)/);
        if (match && match[1]) {
            setSelectedDeviceId(match[1]);
        }
    }, []);

    // Load initial devices on mount
    const loadDevices = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);
            const result = await devicesApi.list(1, 100);
            result.items.forEach(d => addOrUpdate(d));
        } catch (err: any) {
            setError('Failed to load devices');
        } finally {
            setLoading(false);
        }
    }, [addOrUpdate]);

    // Initial load
    useEffect(() => {
        if (!initialLoadedRef.current) {
            initialLoadedRef.current = true;
            loadDevices();
        }
    }, [loadDevices]);

    const [initializedDeviceId, setInitializedDeviceId] = useState<string | null>(null);

    // Load device details when selectedDeviceId changes
    useEffect(() => {
        const selectedDevice = devices.find(d => d.id === selectedDeviceId);

        if (selectedDevice) {
            setDevice(selectedDevice);

            // Only initialize form fields if we haven't for this device yet
            // This prevents overwriting user input when background updates occur (e.g. heartbeats)
            if (selectedDeviceId !== initializedDeviceId) {
                setDeviceName(selectedDevice.deviceName);
                setHeartbeatInterval(selectedDevice.heartbeatIntervalSeconds || 60);
                setInitializedDeviceId(selectedDeviceId);
            }
        } else {
            setDevice(null);
            // Don't reset initializedDeviceId here to avoid re-initializing if device temporarily disappears
        }
    }, [selectedDeviceId, devices, initializedDeviceId]);

    const handleDeviceChange = (deviceId: string) => {
        setSelectedDeviceId(deviceId);
        setSaveError(null);
        setSaveSuccess(null);

        // Update URL without full page reload
        window.history.pushState({}, '', `/device/${deviceId}`);
    };

    const handleUpdateName = async () => {
        if (!device || !deviceName.trim()) {
            setSaveError('Device name cannot be empty');
            return;
        }

        try {
            setSaving('name');
            setSaveError(null);
            setSaveSuccess(null);
            await devicesApi.updateName(device.id, deviceName.trim());
            setSaveSuccess('Device name updated successfully');
            setTimeout(() => setSaveSuccess(null), 3000);
        } catch (err: any) {
            const errorMsg = err.response?.data?.error || 'Failed to update device name';
            setSaveError(errorMsg);
        } finally {
            setSaving(null);
        }
    };

    const handleUpdateHeartbeatInterval = async () => {
        if (!device) return;

        const interval = typeof heartbeatInterval === 'string' ? parseInt(heartbeatInterval) : heartbeatInterval;

        if (!interval || isNaN(interval) || interval < 5 || interval > 3600) {
            setSaveError('Heartbeat interval must be between 5 and 3600 seconds');
            return;
        }

        try {
            setSaving('interval');
            setSaveError(null);
            setSaveSuccess(null);
            await devicesApi.updateHeartbeatInterval(device.id, interval);
            setSaveSuccess('Heartbeat interval updated successfully');
            setTimeout(() => setSaveSuccess(null), 3000);
        } catch (err: any) {
            const errorMsg = err.response?.data?.error || 'Failed to update heartbeat interval';
            setSaveError(errorMsg);
        } finally {
            setSaving(null);
        }
    };

    const formatDate = (dateString: string | null | undefined) => {
        if (!dateString) return 'Never';
        return new Date(dateString).toLocaleString();
    };

    return (
        <div className="page-container">
            <div className="page-header">
                <div>
                    <h1>Device Configuration</h1>
                    <p className="text-muted">Configure device settings and manage device properties</p>
                </div>
                <button
                    className="btn btn-secondary"
                    onClick={loadDevices}
                    disabled={loading}
                >
                    {loading ? 'Refreshing...' : 'Refresh'}
                </button>
            </div>

            {loading ? (
                <p className="text-muted">Loading devices...</p>
            ) : error ? (
                <div className="card">
                    <p style={{ color: 'var(--color-danger)', marginBottom: 'var(--spacing-sm)' }}>
                        {error}
                    </p>
                    <button className="btn btn-primary" onClick={loadDevices}>
                        Retry
                    </button>
                </div>
            ) : devices.length === 0 ? (
                <div className="card">
                    <p className="text-muted">No devices registered yet</p>
                    <p style={{ fontSize: '0.875rem', color: 'var(--color-text-secondary)' }}>
                        Use the BEAM.Emulator to register your first device
                    </p>
                </div>
            ) : (
                <>
                    {/* Device Picker */}
                    <div className="card">
                        <DevicePicker
                            devices={devices}
                            selectedDeviceId={selectedDeviceId}
                            onDeviceChange={handleDeviceChange}
                        />
                    </div>

                    {device && (
                        <>
                            {/* Device Information Card */}
                            <div className="card">
                                <h2 style={{ marginBottom: 'var(--spacing-md)' }}>Device Information</h2>
                                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: 'var(--spacing-md)' }}>
                                    <div>
                                        <label style={{ display: 'block', fontWeight: 600, marginBottom: 'var(--spacing-xs)', color: 'var(--color-text-secondary)' }}>
                                            Device ID
                                        </label>
                                        <code style={{ fontSize: '0.875rem', wordBreak: 'break-all' }}>{device.id}</code>
                                    </div>
                                    <div>
                                        <label style={{ display: 'block', fontWeight: 600, marginBottom: 'var(--spacing-xs)', color: 'var(--color-text-secondary)' }}>
                                            MAC Address
                                        </label>
                                        <code style={{ fontSize: '0.875rem' }}>{device.macAddress}</code>
                                    </div>
                                    <div>
                                        <label style={{ display: 'block', fontWeight: 600, marginBottom: 'var(--spacing-xs)', color: 'var(--color-text-secondary)' }}>
                                            Device Type
                                        </label>
                                        <code style={{ fontSize: '0.875rem' }}>{device.deviceType}</code>
                                    </div>
                                    <div>
                                        <label style={{ display: 'block', fontWeight: 600, marginBottom: 'var(--spacing-xs)', color: 'var(--color-text-secondary)' }}>
                                            Registered At
                                        </label>
                                        <span style={{ fontSize: '0.875rem' }}>{formatDate(device.registeredAt)}</span>
                                    </div>
                                    <div>
                                        <label style={{ display: 'block', fontWeight: 600, marginBottom: 'var(--spacing-xs)', color: 'var(--color-text-secondary)' }}>
                                            Connection Status
                                        </label>
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
                                    <div>
                                        <label style={{ display: 'block', fontWeight: 600, marginBottom: 'var(--spacing-xs)', color: 'var(--color-text-secondary)' }}>
                                            Last Seen
                                        </label>
                                        <span style={{ fontSize: '0.875rem' }}>{formatDate(device.lastSeenAt)}</span>
                                    </div>
                                </div>
                            </div>

                            {/* Configuration Card */}
                            <div className="card">
                                <h2 style={{ marginBottom: 'var(--spacing-md)' }}>Device Settings</h2>

                                {saveError && (
                                    <div style={{ padding: 'var(--spacing-sm)', marginBottom: 'var(--spacing-md)', backgroundColor: '#fef2f2', border: '1px solid #fecaca', borderRadius: 'var(--border-radius)', color: '#991b1b' }}>
                                        {saveError}
                                    </div>
                                )}

                                {saveSuccess && (
                                    <div style={{ padding: 'var(--spacing-sm)', marginBottom: 'var(--spacing-md)', backgroundColor: '#f0fdf4', border: '1px solid #bbf7d0', borderRadius: 'var(--border-radius)', color: '#166534' }}>
                                        {saveSuccess}
                                    </div>
                                )}

                                <div style={{ display: 'grid', gap: 'var(--spacing-lg)' }}>
                                    {/* Device Name */}
                                    <div>
                                        <label htmlFor="device-name" style={{ display: 'block', fontWeight: 600, marginBottom: 'var(--spacing-sm)' }}>
                                            Device Name
                                        </label>
                                        <div style={{ display: 'flex', gap: 'var(--spacing-sm)' }}>
                                            <input
                                                id="device-name"
                                                type="text"
                                                className="form-control"
                                                value={deviceName}
                                                onChange={(e) => setDeviceName(e.target.value)}
                                                disabled={saving === 'name'}
                                                style={{
                                                    flex: 1,
                                                    padding: 'var(--spacing-sm)',
                                                    borderRadius: 'var(--border-radius)',
                                                    border: '1px solid var(--color-border)',
                                                    backgroundColor: 'var(--color-background-elevated)',
                                                    color: 'var(--color-text)',
                                                }}
                                            />
                                            <button
                                                className="btn btn-primary"
                                                onClick={handleUpdateName}
                                                disabled={saving === 'name' || deviceName === device.deviceName}
                                            >
                                                {saving === 'name' ? 'Saving...' : 'Save'}
                                            </button>
                                        </div>
                                        <p style={{ fontSize: '0.875rem', color: 'var(--color-text-secondary)', marginTop: 'var(--spacing-xs)' }}>
                                            Device names must be unique across all devices
                                        </p>
                                    </div>

                                    {/* Heartbeat Interval */}
                                    <div>
                                        <label htmlFor="heartbeat-interval" style={{ display: 'block', fontWeight: 600, marginBottom: 'var(--spacing-sm)' }}>
                                            Heartbeat Interval (seconds)
                                        </label>
                                        <div style={{ display: 'flex', gap: 'var(--spacing-sm)' }}>
                                            <input
                                                id="heartbeat-interval"
                                                type="number"
                                                min="5"
                                                max="3600"
                                                className="form-control"
                                                value={heartbeatInterval}
                                                onChange={(e) => setHeartbeatInterval(e.target.value)}
                                                disabled={saving === 'interval'}
                                                style={{
                                                    flex: 1,
                                                    padding: 'var(--spacing-sm)',
                                                    borderRadius: 'var(--border-radius)',
                                                    border: '1px solid var(--color-border)',
                                                    backgroundColor: 'var(--color-background-elevated)',
                                                    color: 'var(--color-text)',
                                                }}
                                            />
                                            <button
                                                className="btn btn-primary"
                                                onClick={handleUpdateHeartbeatInterval}
                                                disabled={saving === 'interval' || (typeof heartbeatInterval === 'string' ? parseInt(heartbeatInterval) : heartbeatInterval) === device.heartbeatIntervalSeconds}
                                            >
                                                {saving === 'interval' ? 'Saving...' : 'Save'}
                                            </button>
                                        </div>
                                        <p style={{ fontSize: '0.875rem', color: 'var(--color-text-secondary)', marginTop: 'var(--spacing-xs)' }}>
                                            Interval must be between 5 and 3600 seconds
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </>
                    )}
                </>
            )}
        </div>
    );
};
