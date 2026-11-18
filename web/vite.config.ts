import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  return {
    plugins: [react()],
    server: {
      proxy: {
        '/api': {
          // Not VITE_-prefixed on purpose: never exposed to browser code.
          target: env.API_PROXY_TARGET || 'https://localhost:7030',
          changeOrigin: true,
          // Dev server only: Node does not trust the ASP.NET dev certificate.
          secure: false,
        },
      },
    },
  }
})
