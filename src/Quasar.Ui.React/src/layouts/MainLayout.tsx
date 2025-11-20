import React from 'react';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './MainLayout.css';

export const MainLayout: React.FC = () => {
    const { logout } = useAuth();
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
                        <span className="logo-text">BEAM</span>
                    </div>
                </div>

                <nav className="sidebar-nav">
                    <div className="nav-section">
                        <h3 className="nav-section-title">Menu</h3>
                        <Link to="/" className="nav-link">
                            <span>Dashboard</span>
                        </Link>
                    </div>

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
                    </div>
                </nav>

                <div className="sidebar-footer">
                    <button onClick={handleLogout} className="btn btn-secondary w-full btn-sm">
                        Sign Out
                    </button>
                </div>
            </aside>

            {/* Main Content */}
            <main className="main-content">
                <Outlet />
            </main>
        </div>
    );
};
