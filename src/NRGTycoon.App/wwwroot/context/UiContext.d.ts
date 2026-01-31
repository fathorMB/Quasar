import { default as React } from 'react';
export interface UiSettings {
    applicationName: string;
    theme: string;
    logoSymbol?: string;
    customBundleUrl?: string;
    showAdminMenu?: boolean;
}
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
declare global {
    interface Window {
        __QUASAR_CUSTOM_MENU__?: CustomNavSection[];
        __QUASAR_CUSTOM_ROUTES__?: CustomRoute[];
    }
}
interface UiContextValue {
    settings: UiSettings | null;
    isLoading: boolean;
    customMenu: CustomNavSection[];
    customRoutes: CustomRoute[];
}
export declare const UiProvider: React.FC<{
    children: React.ReactNode;
}>;
export declare const useUi: () => UiContextValue;
export {};
