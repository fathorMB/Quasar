import { useEffect, useCallback, useRef, useState } from 'react';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';

/**
 * Represents a read model update notification from the server.
 */
export interface ReadModelUpdate<T> {
    key: string;
    model: T;
}

/**
 * Represents a read model deletion notification from the server.
 */
export interface ReadModelDelete {
    key: string;
}

/**
 * Hook for listening to live read model updates via SignalR.
 * 
 * @template T The read model type
 * @param modelTypeName The name of the read model class (e.g., "DeviceReadModel")
 * @param onUpsert Callback when a model is created or updated
 * @param onDelete Callback when a model is deleted
 * @returns The current connection status ("connecting" | "connected" | "disconnected")
 */
export function useLiveReadModel<T extends object>(
    modelTypeName: string,
    onUpsert: (key: string, model: T) => void,
    onDelete: (key: string) => void
): "connecting" | "connected" | "disconnected" {
    const connectionRef = useRef<HubConnection | null>(null);
    const statusRef = useRef<"connecting" | "connected" | "disconnected">("disconnected");
    const updateCallbackRef = useRef<(key: string, model: T) => void>(onUpsert);
    const deleteCallbackRef = useRef<(key: string) => void>(onDelete);

    // Update callback refs when they change
    useEffect(() => {
        updateCallbackRef.current = onUpsert;
    }, [onUpsert]);

    useEffect(() => {
        deleteCallbackRef.current = onDelete;
    }, [onDelete]);

    useEffect(() => {
        const initConnection = async () => {
            try {
                statusRef.current = "connecting";

                const connection = new HubConnectionBuilder()
                    .withUrl("/hubs/live-models", {
                        withCredentials: true,
                        transport: 1, // WebSocket
                    })
                    .withAutomaticReconnect([0, 0, 1000, 3000, 5000, 10000])
                    .configureLogging(LogLevel.Information)
                    .build();

                // Register handlers for this model type
                connection.on(`ReadModel:Upsert:${modelTypeName}`, (key: string, model: T) => {
                    updateCallbackRef.current(key, model);
                });

                connection.on(`ReadModel:Delete:${modelTypeName}`, (key: string) => {
                    deleteCallbackRef.current(key);
                });

                // Handle connection state changes
                connection.onreconnecting(() => {
                    statusRef.current = "connecting";
                });

                connection.onreconnected(() => {
                    statusRef.current = "connected";
                });

                connection.onclose(() => {
                    statusRef.current = "disconnected";
                });

                await connection.start();
                statusRef.current = "connected";
                connectionRef.current = connection;

                console.log(`Connected to live updates for ${modelTypeName}`);
            } catch (error) {
                console.error("Failed to connect to live updates:", error);
                statusRef.current = "disconnected";
                // Retry connection after delay
                setTimeout(initConnection, 5000);
            }
        };

        initConnection();

        return () => {
            if (connectionRef.current) {
                connectionRef.current.stop().catch(err => console.error("Error stopping connection:", err));
                connectionRef.current = null;
            }
        };
    }, [modelTypeName]);

    return statusRef.current;
}

/**
 * Hook for managing a collection of live read models.
 * 
 * @template T The read model type
 * @param modelTypeName The name of the read model class (e.g., "DeviceReadModel")
 * @param initialModels Optional initial models to populate the collection
 * @returns Object containing models map, add/update/delete methods, and connection status
 */
export function useLiveReadModelCollection<T extends { id: string }>(
    modelTypeName: string,
    initialModels?: T[]
) {
    const [models, setModels] = useState<Map<string, T>>(
        () => new Map(initialModels?.map(m => [m.id, m]) ?? [])
    );

    const handleUpsert = useCallback((key: string, model: T) => {
        setModels(prev => new Map(prev).set(key, model));
    }, []);

    const handleDelete = useCallback((key: string) => {
        setModels(prev => {
            const copy = new Map(prev);
            copy.delete(key);
            return copy;
        });
    }, []);

    const status = useLiveReadModel(modelTypeName, handleUpsert, handleDelete);

    return {
        models: Array.from(models.values()),
        modelsMap: models,
        addOrUpdate: (model: T) => handleUpsert(model.id, model),
        remove: (id: string) => handleDelete(id),
        clear: () => setModels(new Map()),
        status,
        isConnected: status === "connected",
    };
}
