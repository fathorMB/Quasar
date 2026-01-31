import { Feature } from './types';
export declare const featuresApi: {
    list(): Promise<Feature[]>;
    listDirect(): Promise<Feature[]>;
};
