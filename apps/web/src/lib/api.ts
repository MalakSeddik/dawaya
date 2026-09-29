import { createDawayaClient } from 'api-client';

// Same-origin by default: Vite proxies /api to the API in development.
// Set VITE_API_BASE_URL when the API is served from another origin.
export const api = createDawayaClient({
  baseUrl: import.meta.env.VITE_API_BASE_URL ?? window.location.origin,
  // Resolve fetch per call (not once at startup) so it can be swapped in tests or by a native shell.
  fetch: (request) => globalThis.fetch(request),
});
