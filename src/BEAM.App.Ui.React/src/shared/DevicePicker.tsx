import React from 'react';
import type { Device } from '../api/devices';

interface DevicePickerProps {
    devices: Device[];
    selectedDeviceId: string | null;
    onDeviceChange: (deviceId: string) => void;
}

export const DevicePicker: React.FC<DevicePickerProps> = ({
    devices,
    selectedDeviceId,
    onDeviceChange
}) => {
    return (
        <div className="form-group">
            <label htmlFor="device-picker" style={{ fontWeight: 600, marginBottom: 'var(--spacing-sm)' }}>
                Select Device
            </label>
            <select
                id="device-picker"
                className="form-control"
                value={selectedDeviceId || ''}
                onChange={(e) => onDeviceChange(e.target.value)}
                style={{
                    padding: 'var(--spacing-sm)',
                    borderRadius: 'var(--border-radius)',
                    border: '1px solid var(--color-border)',
                    backgroundColor: 'var(--color-background-elevated)',
                    color: 'var(--color-text)',
                    fontSize: '1rem',
                    width: '100%',
                }}
            >
                {!selectedDeviceId && <option value="">-- Select a device --</option>}
                {devices.map((device) => (
                    <option key={device.id} value={device.id}>
                        {device.isConnected ? '🟢' : '🔴'} {device.deviceName}
                    </option>
                ))}
            </select>
        </div>
    );
};
