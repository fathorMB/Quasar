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
    outDir: '../BEAM.Server/BEAM.Server/wwwroot',
    emptyOutDir: true,
  },
})
