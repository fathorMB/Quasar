import React from 'react';

export type CustomNavSection = {
    title?: string;
    items: Array<{
        label: string;
        path: string;
        roles?: string[];
        feature?: string;
    }>;
};

export type CustomRoute = {
    path: string;
    component: React.ComponentType<any>;
    index?: boolean;
    roles?: string[];
    feature?: string;
};
