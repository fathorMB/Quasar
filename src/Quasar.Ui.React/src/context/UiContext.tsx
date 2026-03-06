import React, { createContext, useContext, useState, useEffect } from 'react';

export interface UiSettings {
    applicationName: string;
    theme: string;
    logoSymbol?: string;
    customBundleUrl?: string;
    showAdminMenu?: boolean;
    requireAuthentication?: boolean;
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

export type CustomSidebarAction = {
    label: string;
    variant?: 'primary' | 'secondary' | 'danger';
    onClick: () => void;
};

declare global {
    interface Window {
        __QUASAR_CUSTOM_MENU__?: CustomNavSection[];
        __QUASAR_CUSTOM_ROUTES__?: CustomRoute[];
        __QUASAR_CUSTOM_HEADER__?: React.ComponentType;
        __QUASAR_CUSTOM_ACTIONS__?: CustomSidebarAction[];
        __QUASAR_CUSTOM_OVERLAY__?: React.ComponentType;
    }
}

interface UiContextValue {
    settings: UiSettings | null;
    isLoading: boolean;
    customMenu: CustomNavSection[];
    customRoutes: CustomRoute[];
    customHeaderComponent: React.ComponentType | null;
    customActions: CustomSidebarAction[];
    customOverlayComponent: React.ComponentType | null;
}

const UiContext = createContext<UiContextValue | undefined>(undefined);

export const UiProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [settings, setSettings] = useState<UiSettings | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [customMenu, setCustomMenu] = useState<CustomNavSection[]>([]);
    const [customRoutes, setCustomRoutes] = useState<CustomRoute[]>([]);
    const [customHeaderComponent, setCustomHeaderComponent] = useState<React.ComponentType | null>(null);
    const [customActions, setCustomActions] = useState<CustomSidebarAction[]>([]);
    const [customOverlayComponent, setCustomOverlayComponent] = useState<React.ComponentType | null>(null);

    useEffect(() => {
        const fetchSettings = async () => {
            try {
                const response = await fetch('/api/config/ui');
                if (response.ok) {
                    const data = await response.json();
                    setSettings(data);

                    // Apply theme to HTML element
                    const themeClass = `theme-${data.theme || 'dark'}`;
                    document.documentElement.className = themeClass;
                }
            } catch (error) {
                console.error('Failed to fetch UI settings:', error);
                // Apply default theme
                document.documentElement.className = 'theme-dark';
            } finally {
                setIsLoading(false);
            }
        };

        fetchSettings();
        if (Array.isArray(window.__QUASAR_CUSTOM_MENU__)) {
            setCustomMenu(window.__QUASAR_CUSTOM_MENU__!);
        }
        if (Array.isArray(window.__QUASAR_CUSTOM_ROUTES__)) {
            setCustomRoutes(window.__QUASAR_CUSTOM_ROUTES__!);
        }
        if (window.__QUASAR_CUSTOM_HEADER__) {
            setCustomHeaderComponent(() => window.__QUASAR_CUSTOM_HEADER__!);
        }
        if (Array.isArray(window.__QUASAR_CUSTOM_ACTIONS__)) {
            setCustomActions(window.__QUASAR_CUSTOM_ACTIONS__!);
        }
        if (window.__QUASAR_CUSTOM_OVERLAY__) {
            setCustomOverlayComponent(() => window.__QUASAR_CUSTOM_OVERLAY__!);
        }
    }, []);

    // Re-read globals when the custom bundle finishes loading
    useEffect(() => {
        const applyGlobals = () => {
            if (Array.isArray(window.__QUASAR_CUSTOM_MENU__)) {
                setCustomMenu(window.__QUASAR_CUSTOM_MENU__!);
            }
            if (Array.isArray(window.__QUASAR_CUSTOM_ROUTES__)) {
                setCustomRoutes(window.__QUASAR_CUSTOM_ROUTES__!);
            }
            if (window.__QUASAR_CUSTOM_HEADER__) {
                setCustomHeaderComponent(() => window.__QUASAR_CUSTOM_HEADER__!);
            }
            if (Array.isArray(window.__QUASAR_CUSTOM_ACTIONS__)) {
                setCustomActions(window.__QUASAR_CUSTOM_ACTIONS__!);
            }
            if (window.__QUASAR_CUSTOM_OVERLAY__) {
                setCustomOverlayComponent(() => window.__QUASAR_CUSTOM_OVERLAY__!);
            }
        };

        // Apply immediately if bundle already loaded
        if (window.__QUASAR_BUNDLE_LOADED__) {
            applyGlobals();
        }

        // Also listen for the event in case bundle loads after mount
        window.addEventListener('quasar-bundle-loaded', applyGlobals);
        return () => window.removeEventListener('quasar-bundle-loaded', applyGlobals);
    }, []);

    return (
        <UiContext.Provider value={{ settings, isLoading, customMenu, customRoutes, customHeaderComponent, customActions, customOverlayComponent }}>
            {children}
        </UiContext.Provider>
    );
};

export const useUi = () => {
    const context = useContext(UiContext);
    if (!context) {
        throw new Error('useUi must be used within a UiProvider');
    }
    return context;
};
