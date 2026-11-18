const DEFAULT_API_BASE_URL = '/api'

const configured = import.meta.env.VITE_API_BASE_URL?.trim()

/** API base URL without trailing slashes. Join paths as `${apiBaseUrl}/analytics/...`. */
export const apiBaseUrl: string = (
  configured ? configured : DEFAULT_API_BASE_URL
).replace(/\/+$/, '')
