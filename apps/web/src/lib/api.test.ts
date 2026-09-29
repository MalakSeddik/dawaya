import { afterEach, describe, expect, it, vi } from 'vitest';

import { authResponse, json, mockApi, problem, profile, signedInStorage } from '@/test/mockApi';

import { createApiContext } from './api';

describe('authenticated API client', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('refreshes once and retries when the access token is rejected', async () => {
    let refreshes = 0;
    const calls = mockApi({
      'POST /api/v1/auth/refresh': () => {
        refreshes += 1;
        return json(authResponse({}, `access-${refreshes}`));
      },
      'GET /api/v1/profiles': (call) =>
        call.authorization === 'Bearer access-2' ? json([profile()]) : problem(401, 'unauthorized'),
    });
    const { api } = createApiContext(signedInStorage());

    const { data } = await api.GET('/api/v1/profiles');

    expect(data).toHaveLength(1);
    expect(refreshes).toBe(2);
    expect(calls.filter((c) => c.path === '/api/v1/profiles').map((c) => c.authorization)).toEqual([
      'Bearer access-1',
      'Bearer access-2',
    ]);
  });

  it('signs out when refreshing fails, instead of looping', async () => {
    mockApi({
      'POST /api/v1/auth/refresh': () => problem(401, 'invalid_refresh_token'),
      'GET /api/v1/profiles': () => problem(401, 'unauthorized'),
    });
    const { api, session } = createApiContext(signedInStorage());

    const { response } = await api.GET('/api/v1/profiles');

    expect(response.status).toBe(401);
    expect(session.getSnapshot().status).toBe('signedOut');
  });

  it('sends no token for guests', async () => {
    const calls = mockApi({ 'GET /api/v1/ingredients': () => json([]) });
    const { api } = createApiContext();

    await api.GET('/api/v1/ingredients', { params: { query: { search: 'pen' } } });

    expect(calls[0]?.authorization).toBeNull();
  });
});
