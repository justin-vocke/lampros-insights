/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Optional API base URL. Defaults to "/api" (proxied by the Vite dev server). */
  readonly VITE_API_BASE_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
