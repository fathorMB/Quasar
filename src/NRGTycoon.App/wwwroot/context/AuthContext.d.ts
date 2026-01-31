import { default as React, ReactNode } from 'react';
import { User, LoginRequest } from '../api';
interface AuthContextType {
    user: User | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    login: (credentials: LoginRequest) => Promise<void>;
    logout: () => Promise<void>;
}
export declare const useAuth: () => AuthContextType;
interface AuthProviderProps {
    children: ReactNode;
}
export declare const AuthProvider: React.FC<AuthProviderProps>;
export {};
