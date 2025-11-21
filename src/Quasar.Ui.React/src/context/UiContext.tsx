import React, { createContext, useContext, useState, useEffect } from 'react';

interface UiSettings {
    applicationName: string;
    theme: string;
    logoSymbol?: string;
}

interface UiContextValue {
    settings: UiSettings | null;
    isLoading: boolean;
}

const UiContext = createContext<UiContextValue | undefined>(undefined);

export const UiProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [settings, setSettings] = useState<UiSettings | null>(null);
    const [isLoading, setIsLoading] = useState(true);

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
    }, []);

    return (
        <UiContext.Provider value={{ settings, isLoading }}>
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
