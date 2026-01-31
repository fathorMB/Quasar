import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { resolve } from 'path'

// https://vite.dev/config/
export default defineConfig({
    plugins: [
        react()
    ],
    server: {
        port: 5173,
        proxy: {
            '/auth': {
                target: 'http://localhost:5288',
                changeOrigin: true,
                secure: false
            },
            '/api': {
                target: 'http://localhost:5288',
                changeOrigin: true,
                secure: false
            }
        }
    },
    build: {
        outDir: 'dist-app',
        emptyOutDir: true,
        // Standard SPA build, no library mode
    },
})
