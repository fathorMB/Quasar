import React from 'react';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useUi } from '../context/UiContext';
import { useFeatures } from '../context/FeatureContext';
import { Header } from '../components/Header';
import './MainLayout.css';

export const MainLayout: React.FC = () => {
    const { user, logout } = useAuth();
    const { settings, customMenu } = useUi();
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
                        <div className="logo-icon">{settings?.logoSymbol || 'Q'}</div>
                        <span className="logo-text">{settings?.applicationName || 'BEAM'}</span>
                    </div>
                </div>


                <nav className="sidebar-nav">
                    {customMenu.map((section, idx) => (
                        <div className="nav-section" key={`custom-${idx}`}>
                            {section.title && <h3 className="nav-section-title">{section.title}</h3>}
                            {section.items.map((item, i) => {
                                if (item.roles && !item.roles.some(r => user?.roles?.includes(r))) {
                                    return null;
                                }
                                if (item.feature && !hasFeature(item.feature)) {
                                    return null;
                                }
                                return (
                                    <Link to={item.path} className="nav-link" key={`custom-item-${i}-${item.path}`}>
                                        <span>{item.label}</span>
                                    </Link>
                                );
                            })}
                        </div>
                    ))}

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
                            <Link to="/logs" className="nav-link">
                                <span>Logs</span>
                            </Link>
                            <Link to="/sessions" className="nav-link">
                                <span>Sessions</span>
                            </Link>
                            {hasFeature('scheduler') && (
                                <Link to="/jobs" className="nav-link">
                                    <span>Jobs</span>
                                </Link>
                            )}
                            {hasFeature('telemetry') && (
                                <Link to="/metrics" className="nav-link">
                                    <span>Metrics</span>
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
