import React, { useEffect } from 'react';

/**
 * Forcefully locks the scroll on the page and common layout containers.
 * Useful for immersive pages like maps or editors.
 */
export const PageScrollLock: React.FC = () => {
    useEffect(() => {
        // Find existing style or create one
        let style = document.getElementById('quasar-scroll-lock') as HTMLStyleElement;
        if (!style) {
            style = document.createElement('style');
            style.id = 'quasar-scroll-lock';
            document.head.appendChild(style);
        }

        // Lock overflow on all potential containers except the sidebar
        // This targets the main-content wrapper specifically in MainLayout
        style.innerHTML = `
            html, body, #root, .content-wrapper, .main-content {
                overflow: hidden !important;
                height: 100% !important;
                max-height: 100% !important;
            }
        `;

        return () => {
            const el = document.getElementById('quasar-scroll-lock');
            if (el) el.remove();
        };
    }, []);

    return null;
};
