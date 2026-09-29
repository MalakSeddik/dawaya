import { describe, expect, it, vi } from 'vitest';

import { createMemoryStorage } from '@/lib/platform/storage';
import { authResponse, signedInStorage } from '@/test/mockApi';

import { SESSION_STORAGE_KEY, SessionStore } from './session';

describe('SessionStore', () => {
  it('starts signed out when nothing is saved, and loading when a session is saved', () => {
    expect(new SessionStore(createMemoryStorage(), vi.fn()).getSnapshot().status).toBe('signedOut');
    expect(new SessionStore(signedInStorage(), vi.fn()).getSnapshot().status).toBe('loading');
  });

  it('keeps only the refresh token in storage, never the access token', () => {
    const storage = createMemoryStorage();
    const store = new SessionStore(storage, vi.fn());

    store.signIn(authResponse({}, 'secret-access'));

    const saved = storage.getItem(SESSION_STORAGE_KEY) ?? '';
    expect(saved).toContain('refresh-secret-access');
    expect(saved).not.toContain('"secret-access"');
    expect(store.getSnapshot()).toMatchObject({ status: 'signedIn', user: { email: 'malak@example.test' } });
  });

  it('shares one refresh between concurrent callers (rotation would otherwise look like theft)', async () => {
    const refresh = vi.fn(() => Promise.resolve(authResponse()));
    const store = new SessionStore(signedInStorage(), refresh);

    const results = await Promise.all([store.getAccessToken(), store.getAccessToken(), store.refresh()]);

    expect(refresh).toHaveBeenCalledTimes(1);
    expect(results[0]).toBe('access-1');
    expect(results[1]).toBe('access-1');
  });

  it('two tabs refreshing at once never send the same token twice', async () => {
    // Both "tabs" share storage and a cross-tab lock, like two browser tabs of the app.
    const storage = signedInStorage();
    let queue = Promise.resolve();
    const sharedLock = <T,>(work: () => Promise<T>): Promise<T> => {
      const run = queue.then(work);
      queue = run.then(
        () => undefined,
        () => undefined,
      );
      return run;
    };
    let counter = 0;
    const used: string[] = [];
    // A fake server that rotates tokens and rejects any reuse, like the real API.
    const server = (token: string) => {
      if (used.includes(token)) return Promise.resolve(null);
      used.push(token);
      counter += 1;
      return Promise.resolve(authResponse({}, `access-${counter}`));
    };
    const tabA = new SessionStore(storage, server, Date.now, sharedLock);
    const tabB = new SessionStore(storage, server, Date.now, sharedLock);

    const [a, b] = await Promise.all([tabA.refresh(), tabB.refresh()]);

    expect([a, b]).toEqual([true, true]);
    expect(new Set(used).size).toBe(used.length);
    expect(tabA.getSnapshot().status).toBe('signedIn');
    expect(tabB.getSnapshot().status).toBe('signedIn');
  });

  it('signs out when the server rejects the refresh token', async () => {
    const storage = signedInStorage();
    const store = new SessionStore(storage, () => Promise.resolve(null));

    expect(await store.refresh()).toBe(false);

    expect(store.getSnapshot().status).toBe('signedOut');
    expect(store.hasRefreshToken()).toBe(false);
  });

  it('keeps the saved session when the network is down', async () => {
    const storage = signedInStorage();
    const store = new SessionStore(storage, () => Promise.reject(new Error('offline')));

    expect(await store.refresh()).toBe(false);

    expect(store.hasRefreshToken()).toBe(true);
  });

  it('can restore the session later after a network failure', async () => {
    let online = false;
    const store = new SessionStore(signedInStorage(), () =>
      online ? Promise.resolve(authResponse()) : Promise.reject(new Error('offline')),
    );
    await store.refresh();
    expect(store.getSnapshot().status).toBe('signedOut');

    online = true;
    await store.refresh();

    expect(store.getSnapshot().status).toBe('signedIn');
  });

  it('ignores an expired saved session', () => {
    const storage = createMemoryStorage({
      [SESSION_STORAGE_KEY]: JSON.stringify({ refreshToken: 'old', refreshTokenExpiresAt: '2020-01-01T00:00:00Z' }),
    });

    expect(new SessionStore(storage, vi.fn()).getSnapshot().status).toBe('signedOut');
  });
});
