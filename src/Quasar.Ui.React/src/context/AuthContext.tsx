import React, { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import { authApi, usersApi, type User, type LoginRequest } from '../api';

interface AuthContextType {
    user: User | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    login: (credentials: LoginRequest) => Promise<void>;
    logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};

interface AuthProviderProps {
    children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
    const [user, setUser] = useState<User | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    // We can't use `useUi` directly here because AuthProvider might be wrapping UiProvider or vice-versa
    // or they might be siblings. To be safe, we'll fetch the config directly in AuthContext just like UiContext does,
    // or we can wait for UiContext to provide it if we restructure.
    // Instead of restructuring, let's just fetch the config directly here to decide if we need auth.

    useEffect(() => {
        let isMounted = true;
        const initAuth = async () => {
            try {
                // First check if auth is required
                const response = await fetch('/api/config/ui');
                if (response.ok) {
                    const data = await response.json();
                    if (data.requireAuthentication === false) {
                        // Mock an authenticated user
                        if (isMounted) {
                            localStorage.setItem('quasar_auth_mocked', 'true');
                            setUser({
                                id: '00000000-0000-0000-0000-000000000000',
                                username: data.applicationName || 'Local User',
                                email: 'local@localhost',
                                roles: ['administrator'] // Give admin access so they can see everything
                            });
                            setIsLoading(false);
                        }
                        return; // Bypass normal auth check
                    }
                }
            } catch (error) {
                console.error('Failed to fetch UI settings for auth check:', error);
            }

            // Proceed with normal auth check
            if (isMounted) {
                await checkAuth();
            }
        };

        initAuth();
        return () => { isMounted = false; };
    }, []);

    const checkAuth = async () => {
        try {
            const token = localStorage.getItem('accessToken');
            if (token) {
                const payload = parseJwt(token);
                console.log('Decoded payload:', payload);
                const userId = payload.sub;
                const username = payload.unique_name || payload.name || 'User';

                // Fetch roles
                let roles: string[] = [];
                try {
                    console.log('Fetching roles for userId:', userId);
                    const userRoles = await usersApi.getRoles(userId);
                    console.log('Fetched userRoles:', userRoles);
                    roles = userRoles.map(r => r.name);
                    console.log('Mapped roles:', roles);
                } catch (err) {
                    console.error('Failed to fetch roles', err);
                }

                setUser({
                    id: userId,
                    username: username,
                    email: payload.email || '',
                    roles: roles
                });
            } else {
                setUser(null);
            }
        } catch (error) {
            console.error('Auth check failed:', error);
            setUser(null);
        } finally {
            setIsLoading(false);
        }
    };

    function parseJwt(token: string) {
        var base64Url = token.split('.')[1];
        var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        return JSON.parse(jsonPayload);
    }

    const login = async (credentials: LoginRequest) => {
        setIsLoading(true);
        try {
            await authApi.login(credentials);
            // Refresh auth state
            await checkAuth();
        } catch (error) {
            setUser(null);
            throw error;
        } finally {
            setIsLoading(false);
        }
    };

    const logout = async () => {
        setIsLoading(true);
        try {
            await authApi.logout();
        } catch (error) {
            console.error('Logout error:', error);
        } finally {
            setUser(null);
            setIsLoading(false);
        }
    };

    const value: AuthContextType = {
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        logout,
    };

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
