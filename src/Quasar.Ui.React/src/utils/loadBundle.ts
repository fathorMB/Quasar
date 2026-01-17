import type { UiSettings } from '../context/UiContext';

declare global {
    interface Window {
        __QUASAR_BUNDLE_LOADED__?: boolean;
    }
}

export async function fetchUiConfig() {
    try {
        const response = await fetch('/api/config/ui');
        if (!response.ok) return;
        const settings = await response.json() as UiSettings;
        const url = settings.customBundleUrl;
        if (url) {
            ensureProcessEnv();
            await loadModule(url);
        }
    } catch (err) {
        console.error('Failed to load UI config/bundle', err);
    } finally {
        window.__QUASAR_BUNDLE_LOADED__ = true;
    }
}

function ensureProcessEnv() {
    const w = window as any;
    if (!w.process) {
        w.process = { env: { NODE_ENV: 'production' } };
    } else if (!w.process.env) {
        w.process.env = { NODE_ENV: 'production' };
    } else if (!w.process.env.NODE_ENV) {
        w.process.env.NODE_ENV = 'production';
    }
}

async function loadModule(url: string) {
    // Load CSS file first (replace .js with .css)
    const cssUrl = url.replace(/\.js$/, '.css');
    try {
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = cssUrl;
        document.head.appendChild(link);
        console.log('Loaded custom CSS:', cssUrl);
    } catch (err) {
        console.warn('Failed to load custom CSS', cssUrl, err);
    }

    // Use dynamic import to execute the module and register globals.
    try {
        await import(/* @vite-ignore */ url);
    } catch (err) {
        console.error('Failed to load custom UI bundle', err);
    }
}
