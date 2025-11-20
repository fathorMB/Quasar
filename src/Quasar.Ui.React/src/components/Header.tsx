import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Header.css';

export const Header: React.FC = () => {
    const { user, logout } = useAuth();
    const [isMenuOpen, setIsMenuOpen] = React.useState(false);
    const navigate = useNavigate();
    const menuRef = React.useRef<HTMLDivElement>(null);

    const handleLogout = async () => {
        await logout();
        navigate('/login');
    };

    // Close menu when clicking outside
    React.useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
                setIsMenuOpen(false);
            }
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, []);

    return (
        <header className="app-header">
            <div className="header-left">
                {/* Placeholder for page title or breadcrumbs if needed later */}
                <h2 className="page-title">Dashboard</h2>
            </div>

            <div className="header-right">
                <div className="user-profile">
                    <div className="user-info">
                        <span className="user-name">{user?.username || 'Guest'}</span>
                        <span className="user-role">{user?.roles?.[0] || 'User'}</span>
                    </div>
                    <div className="user-avatar">
                        {user?.username?.charAt(0).toUpperCase() || 'U'}
                    </div>
                </div>
                <div className="menu-container" ref={menuRef}>
                    <button
                        className="menu-button"
                        aria-label="Menu"
                        onClick={() => setIsMenuOpen(!isMenuOpen)}
                    >
                        <span className="menu-icon">⋮</span>
                    </button>

                    {isMenuOpen && (
                        <div className="dropdown-menu">
                            <button onClick={handleLogout} className="dropdown-item danger">
                                Sign Out
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </header>
    );
};
