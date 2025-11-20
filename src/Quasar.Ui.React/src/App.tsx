import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { UiProvider } from './context/UiContext';
import { MainLayout } from './layouts/MainLayout';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { UsersPage } from './pages/UsersPage';
import { RolesPage } from './pages/RolesPage';
import { FeaturesPage } from './pages/FeaturesPage';
import './styles/globals.css';
import './styles/components.css';

// Protected Route wrapper
const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center w-full h-full">
        <div className="spinner" style={{ width: '40px', height: '40px' }}></div>
      </div>
    );
  }

  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
};

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <MainLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<DashboardPage />} />
        <Route path="users" element={<UsersPage />} />
        <Route path="roles" element={<RolesPage />} />
        <Route path="features" element={<FeaturesPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

function App() {
  return (
    <BrowserRouter>
      <UiProvider>
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </UiProvider>
    </BrowserRouter>
  );
}

export default App;
