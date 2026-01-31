import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import dts from 'vite-plugin-dts'
import { resolve } from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    dts({
      tsconfigPath: './tsconfig.app.json',
      insertTypesEntry: true
    })
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
    lib: {
      entry: resolve(__dirname, 'src/index.ts'),
      name: 'QuasarUiReact',
      fileName: (format) => `index.${format}.js`
    },
    rollupOptions: {
      external: ['react', 'react-dom', 'react-router-dom', '@microsoft/signalr', 'axios'],
      output: {
        globals: {
          react: 'React',
          'react-dom': 'ReactDOM',
          'react-router-dom': 'ReactRouterDOM',
          '@microsoft/signalr': 'signalR',
          axios: 'axios'
        }
      }
    },
    emptyOutDir: true,
  },
})
