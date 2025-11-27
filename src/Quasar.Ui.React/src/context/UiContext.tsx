import React, { createContext, useContext, useState, useEffect } from 'react';

export interface UiSettings {
    applicationName: string;
    theme: string;
    logoSymbol?: string;
    customBundleUrl?: string;
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

const UiContext = createContext<UiContextValue | undefined>(undefined);

export const UiProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [settings, setSettings] = useState<UiSettings | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [customMenu, setCustomMenu] = useState<CustomNavSection[]>([]);
    const [customRoutes, setCustomRoutes] = useState<CustomRoute[]>([]);

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
    }, []);

    return (
        <UiContext.Provider value={{ settings, isLoading, customMenu, customRoutes }}>
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
