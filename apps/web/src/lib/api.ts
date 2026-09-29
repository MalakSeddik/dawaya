import { createDawayaClient, type DawayaClient, type Middleware } from 'api-client';

import { SessionStore } from '@/features/auth/session';
import { browserStorage, type KeyValueStorage } from '@/lib/platform/storage';

// Same-origin by default: Vite proxies /api to the API in development.
// Set VITE_API_BASE_URL when the API is served from another origin.
const baseUrl = import.meta.env.VITE_API_BASE_URL ?? window.location.origin;
// Resolve fetch per call (not once at startup) so it can be swapped in tests or by a native shell.
const lazyFetch = (request: Request) => globalThis.fetch(request);

export interface ApiContext {
  api: DawayaClient;
  session: SessionStore;
  storage: KeyValueStorage;
}

export function createApiContext(storage: KeyValueStorage = browserStorage): ApiContext {
  // A plain client for /auth/refresh, so refreshing never goes through the auth middleware.
  const plain = createDawayaClient({ baseUrl, fetch: lazyFetch });
  const session = new SessionStore(storage, async (refreshToken) => {
    const { data, response } = await plain.POST('/api/v1/auth/refresh', { body: { refreshToken } });
    if (data) return data;
    if (response.status === 401 || response.status === 400) return null;
    throw new Error(`Refresh failed with HTTP ${response.status}`);
  });

  const api = createDawayaClient({ baseUrl, fetch: lazyFetch });
  api.use(authMiddleware(session));
  return { api, session, storage };
}

const retryCopies = new WeakMap<Request, Request>();

// Adds the access token, and on a 401 refreshes once and retries the request.
function authMiddleware(session: SessionStore): Middleware {
  return {
    async onRequest({ request }) {
      if (isAuthEndpoint(request)) return request;
      const token = await session.getAccessToken();
      if (token) request.headers.set('Authorization', `Bearer ${token}`);
      retryCopies.set(request, request.clone());
      return request;
    },
    async onResponse({ request, response }) {
      if (response.status !== 401 || isAuthEndpoint(request) || !session.hasRefreshToken()) return response;
      const copy = retryCopies.get(request);
      if (!copy || !(await session.refresh())) return response;
      const token = await session.getAccessToken();
      if (token) copy.headers.set('Authorization', `Bearer ${token}`);
      return lazyFetch(copy);
    },
  };
}

function isAuthEndpoint(request: Request): boolean {
  return new URL(request.url).pathname.startsWith('/api/v1/auth/');
}
