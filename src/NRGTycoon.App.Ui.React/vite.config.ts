import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { resolve, dirname } from "path";
import { fileURLToPath } from "url";

const __dirname = dirname(fileURLToPath(import.meta.url));

export default defineConfig({
    plugins: [react()],
    server: {
        port: 5175,
        proxy: {
            "/auth": {
                target: "http://localhost:5290",
                changeOrigin: true,
                secure: false,
            },
            "/api": {
                target: "http://localhost:5290",
                changeOrigin: true,
                secure: false,
            },
        },
    },
    build: {
        lib: {
            entry: resolve(__dirname, "src/nrgtycoon.ts"),
            name: "NRGTycoonUi",
            formats: ["umd"],
            fileName: () => "nrg-ui.js",
        },
        outDir: resolve(__dirname, "dist"),
        emptyOutDir: true,
        rollupOptions: {
            external: ["react", "react-dom", "react-router-dom"],
            output: {
                globals: {
                    react: "React",
                    "react-dom": "ReactDOM",
                    "react-router-dom": "ReactRouterDOM",
                },
            },
        },
    },
});
