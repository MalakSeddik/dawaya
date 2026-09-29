import type { Schemas } from 'api-client';
import { vi } from 'vitest';

import { SESSION_STORAGE_KEY } from '@/features/auth/session';
import { createMemoryStorage } from '@/lib/platform/storage';

export interface RecordedCall {
  method: string;
  path: string;
  body: unknown;
  authorization: string | null;
}

type Handler = (call: RecordedCall) => Response | Promise<Response>;

export function json(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
}

export function problem(status: number, code: string): Response {
  return new Response(JSON.stringify({ status, title: code, code }), {
    status,
    headers: { 'Content-Type': 'application/problem+json' },
  });
}

// Replaces fetch with a router keyed by "METHOD /path". Unknown routes answer 404.
export function mockApi(routes: Record<string, Handler>) {
  const calls: RecordedCall[] = [];
  vi.stubGlobal(
    'fetch',
    vi.fn(async (input: Request) => {
      const url = new URL(input.url);
      const text = await input.clone().text();
      const call: RecordedCall = {
        method: input.method,
        path: url.pathname + url.search,
        body: text ? (JSON.parse(text) as unknown) : undefined,
        authorization: input.headers.get('Authorization'),
      };
      calls.push(call);
      const handler = routes[`${call.method} ${url.pathname}`];
      return handler ? handler(call) : problem(404, 'not_found');
    }),
  );
  return calls;
}

export const testUser: Schemas['CurrentUserResponse'] = {
  id: 'user-1',
  email: 'malak@example.test',
  preferredLanguage: 'ar',
  largeText: false,
  highContrast: false,
  roles: ['User'],
};

export function authResponse(user: Partial<Schemas['CurrentUserResponse']> = {}, token = 'access-1'): Schemas['AuthResponse'] {
  const now = Date.now();
  return {
    accessToken: token,
    accessTokenExpiresAt: new Date(now + 15 * 60_000).toISOString(),
    refreshToken: `refresh-${token}`,
    refreshTokenExpiresAt: new Date(now + 30 * 86_400_000).toISOString(),
    user: { ...testUser, ...user },
  };
}

// Device storage that already holds a saved session, as after a previous sign-in.
export function signedInStorage() {
  return createMemoryStorage({
    [SESSION_STORAGE_KEY]: JSON.stringify({
      refreshToken: 'refresh-saved',
      refreshTokenExpiresAt: new Date(Date.now() + 86_400_000).toISOString(),
    }),
  });
}

export function profile(overrides: Partial<Schemas['ProfileResponse']> = {}): Schemas['ProfileResponse'] {
  return {
    id: 1,
    displayName: 'Malak',
    relationship: null,
    isSelf: true,
    ageGroup: null,
    isPregnant: false,
    isBreastfeeding: false,
    allergies: [],
    createdAt: '2026-09-01T08:00:00Z',
    updatedAt: '2026-09-01T08:00:00Z',
    ...overrides,
  };
}
