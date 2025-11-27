import axios from 'axios';

export interface Device {
    id: string;
    deviceName: string;
    deviceType: string;
    macAddress: string;
    isActive: boolean;
    isConnected: boolean;
    registeredAt: string;
    lastSeenAt?: string;
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
}

export const devicesApi = {
    async list(page: number = 1, pageSize: number = 20): Promise<PagedResult<Device>> {
        const response = await axios.get(`/api/devices`, {
            params: { page, pageSize }
        });
        return response.data;
    },

    async get(id: string): Promise<Device> {
        const response = await axios.get(`/api/devices/${id}`);
        return response.data;
    }
};
