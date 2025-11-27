import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { UiProvider, useUi } from './context/UiContext';
import { FeatureProvider } from './context/FeatureContext';
import { useFeatures } from './context/FeatureContext';
import './styles/modal-fix.css';
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
const ProtectedRoute: React.FC<{ children: React.ReactNode; roles?: string[]; feature?: string }> = ({ children, roles, feature }) => {
  const { isAuthenticated, isLoading, user } = useAuth();
  const { hasFeature } = useFeatures();
  const { settings } = useUi();

  const ready = settings?.customBundleUrl === undefined || settings?.customBundleUrl === null || settings?.customBundleUrl === '' || (window as any).__QUASAR_BUNDLE_LOADED__ === true;

  if (isLoading || !ready) {
    return (
      <div className="flex items-center justify-center w-full h-full">
        <div className="spinner" style={{ width: '40px', height: '40px' }}></div>
      </div>
    );
  }

  if (roles && roles.length > 0 && !roles.some(r => user?.roles?.includes(r))) {
    return <Navigate to="/" replace />;
  }

  if (feature && !hasFeature(feature)) {
    return <Navigate to="/" replace />;
  }

  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
};

function AppRoutes() {
  const { customRoutes } = useUi();

  const customIndex = customRoutes.find(r => r.index);

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
        <Route
          index
          element={
            customIndex ? (() => {
              const Element = customIndex.component;
              return <Element />;
            })() : <DashboardPage />
          }
        />
        <Route path="users" element={<UsersPage />} />
        <Route path="roles" element={<RolesPage />} />
        <Route path="features" element={<FeaturesPage />} />
        <Route path="jobs" element={<JobsPage />} />
        <Route path="logs" element={<LogsPage />} />
        <Route path="metrics" element={<MetricsPage />} />
        <Route path="sessions" element={<SessionsPage />} />
        {customRoutes.map((route, idx) => {
          const Element = route.component;
          return (
            <Route
              key={`custom-route-${idx}-${route.path}`}
              path={route.path}
              element={
                <ProtectedRoute roles={route.roles} feature={route.feature}>
                  <Element />
                </ProtectedRoute>
              }
            />
          );
        })}
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
