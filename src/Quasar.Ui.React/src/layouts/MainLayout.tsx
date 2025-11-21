import React from 'react';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useUi } from '../context/UiContext';
import { useFeatures } from '../context/FeatureContext';
import { Header } from '../components/Header';
import './MainLayout.css';

export const MainLayout: React.FC = () => {
    const { user, logout } = useAuth();
    const { settings } = useUi();
    const { hasFeature } = useFeatures();
    const navigate = useNavigate();

    const handleLogout = async () => {
        await logout();
        navigate('/login');
    };

    return (
        <div className="main-layout">
            {/* Sidebar Navigation */}
            <aside className="sidebar">
                <div className="sidebar-header">
                    <div className="logo">
                        <div className="logo-icon">Q</div>
                        <span className="logo-text">{settings?.applicationName || 'BEAM'}</span>
                    </div>
                </div>

                <nav className="sidebar-nav">
                    <div className="nav-section">
                        <h3 className="nav-section-title">Menu</h3>
                        <Link to="/" className="nav-link">
                            <span>Dashboard</span>
                        </Link>
                    </div>

                    {user?.roles?.includes('administrator') && (
                        <div className="nav-section">
                            <h3 className="nav-section-title">Administration</h3>
                            <Link to="/users" className="nav-link">
                                <span>Users</span>
                            </Link>
                            <Link to="/roles" className="nav-link">
                                <span>Roles</span>
                            </Link>
                            <Link to="/features" className="nav-link">
                                <span>Features</span>
                            </Link>
                            {hasFeature('scheduler') && (
                                <Link to="/jobs" className="nav-link">
                                    <span>Jobs</span>
                                </Link>
                            )}
                        </div>
                    )}
                </nav>

                <div className="sidebar-footer">
                    <button onClick={handleLogout} className="btn btn-secondary w-full btn-sm">
                        Sign Out
                    </button>
                </div>
            </aside>

            {/* Main Content Wrapper */}
            <div className="content-wrapper">
                <Header />
                <main className="main-content">
                    <Outlet />
                </main>
            </div>
        </div>
    );
};
