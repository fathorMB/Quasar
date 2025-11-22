import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { UiProvider } from './context/UiContext';
import { FeatureProvider } from './context/FeatureContext';
import { MainLayout } from './layouts/MainLayout';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { UsersPage } from './pages/UsersPage';
import { RolesPage } from './pages/RolesPage';
import { FeaturesPage } from './pages/FeaturesPage';
import { JobsPage } from './pages/JobsPage';
import { LogsPage } from './pages/LogsPage';
import { MetricsPage } from './pages/MetricsPage';
import { SessionsPage } from './pages/SessionsPage';
import { SessionRevokedModal } from './components/SessionRevokedModal';
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
        <Route path="jobs" element={<JobsPage />} />
        <Route path="logs" element={<LogsPage />} />
        <Route path="metrics" element={<MetricsPage />} />
        <Route path="sessions" element={<SessionsPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

function App() {
  const [showSessionRevokedModal, setShowSessionRevokedModal] = React.useState(false);

  React.useEffect(() => {
    const handleSessionRevoked = () => {
      setShowSessionRevokedModal(true);
    };

    window.addEventListener('session-revoked', handleSessionRevoked);
    return () => window.removeEventListener('session-revoked', handleSessionRevoked);
  }, []);

  const handleModalClose = () => {
    setShowSessionRevokedModal(false);
    window.location.href = '/login';
  };

  return (
    <BrowserRouter>
      <UiProvider>
        <AuthProvider>
          <FeatureProvider>
            <AppRoutes />
            {showSessionRevokedModal && (
              <SessionRevokedModal onClose={handleModalClose} />
            )}
          </FeatureProvider>
        </AuthProvider>
      </UiProvider>
    </BrowserRouter>
  );
}

export default App;
