import React, { createContext, useContext, useState, useEffect } from 'react';
import { featuresApi } from '../api/features';
import type { Feature } from '../api/types';

interface FeatureContextValue {
    features: Feature[];
    isLoading: boolean;
    hasFeature: (id: string) => boolean;
}

const FeatureContext = createContext<FeatureContextValue | undefined>(undefined);

export const FeatureProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [features, setFeatures] = useState<Feature[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const fetchFeatures = async () => {
            try {
                const data = await featuresApi.list();
                setFeatures(data);
            } catch (error) {
                console.error('Failed to fetch features:', error);
            } finally {
                setIsLoading(false);
            }
        };

        fetchFeatures();
    }, []);

    const hasFeature = (id: string) => features.some(f => f.id === id);

    return (
        <FeatureContext.Provider value={{ features, isLoading, hasFeature }}>
            {children}
        </FeatureContext.Provider>
    );
};

export const useFeatures = () => {
    const context = useContext(FeatureContext);
    if (!context) {
        throw new Error('useFeatures must be used within a FeatureProvider');
    }
    return context;
};
