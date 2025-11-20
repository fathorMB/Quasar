import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/auth': {
        target: 'http://localhost:5289',
        changeOrigin: true,
        secure: false,
      },
    },
  },
  build: {
    // Keep the SPA build inside this project so BEAM.App's MSBuild targets
    // can find the expected dist/ folder to copy into its wwwroot.
    outDir: 'dist',
    emptyOutDir: true,
  },
})
