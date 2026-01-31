import { default as React } from 'react';
import { Feature } from '../api/types';
interface FeatureContextValue {
    features: Feature[];
    isLoading: boolean;
    hasFeature: (id: string) => boolean;
}
export declare const FeatureProvider: React.FC<{
    children: React.ReactNode;
}>;
export declare const useFeatures: () => FeatureContextValue;
export {};
