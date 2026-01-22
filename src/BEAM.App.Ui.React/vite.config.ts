import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5174,
    proxy: {
      "/auth": {
        target: "http://localhost:5288",
        changeOrigin: true,
        secure: false,
      },
      "/api": {
        target: "http://localhost:5288",
        changeOrigin: true,
        secure: false,
      },
    },
  },
  build: {
    lib: {
      entry: path.resolve(__dirname, "src/beam.ts"),
      name: "BeamUi",
      formats: ["umd"],
      fileName: () => "beam-ui.js",
    },
    outDir: path.resolve(__dirname, "dist"),
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
