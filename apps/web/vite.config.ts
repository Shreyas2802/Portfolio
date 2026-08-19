import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    // In dev, the React SPA runs on Vite's dev server and proxies API
    // calls to the ASP.NET Core API so the browser only ever talks to
    // one origin. Change the target port if your API launchSettings
    // use a different one.
    proxy: {
      '/api': {
        target: 'http://localhost:5199',
        changeOrigin: true,
      },
    },
  },
})
