import type { Schemas } from 'api-client';

import type { KeyValueStorage } from '@/lib/platform/storage';

export type AuthResponse = Schemas['AuthResponse'];
export type CurrentUser = Schemas['CurrentUserResponse'];

export const SESSION_STORAGE_KEY = 'dawaya.session';

export type SessionStatus = 'loading' | 'signedIn' | 'signedOut';

export interface SessionSnapshot {
  status: SessionStatus;
  user: CurrentUser | null;
}

interface StoredSession {
  refreshToken: string;
  refreshTokenExpiresAt: string;
}

// Calls POST /api/v1/auth/refresh without going through the authenticated client (avoids loops).
export type RefreshCall = (refreshToken: string) => Promise<AuthResponse | null>;

// Runs work while holding a lock shared by every open tab of the app.
export type CrossTabLock = <T>(work: () => Promise<T>) => Promise<T>;

// Refresh tokens rotate, and the server treats a reused token as theft. Tabs share one saved token,
// so they must take turns: the Web Locks API gives one lock across all tabs of this origin.
export const browserRefreshLock: CrossTabLock = (work) =>
  typeof navigator !== 'undefined' && 'locks' in navigator
    ? navigator.locks.request('dawaya-session-refresh', work)
    : work();

// Holds the signed-in session outside React so the API client can use it.
// The access token lives only in memory; the refresh token is kept in device storage so the user
// stays signed in (a native app swaps the storage for secure storage, CLAUDE.md rule 5).
export class SessionStore {
  private accessToken: string | null = null;
  private accessTokenExpiresAt = 0;
  private snapshot: SessionSnapshot;
  private readonly listeners = new Set<() => void>();
  private inflightRefresh: Promise<boolean> | null = null;

  constructor(
    private readonly storage: KeyValueStorage,
    private readonly refreshCall: RefreshCall,
    private readonly now: () => number = () => Date.now(),
    private readonly lock: CrossTabLock = browserRefreshLock,
  ) {
    this.snapshot = { status: this.readStored() ? 'loading' : 'signedOut', user: null };
  }

  getSnapshot = (): SessionSnapshot => this.snapshot;

  subscribe = (listener: () => void): (() => void) => {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  };

  hasRefreshToken(): boolean {
    return this.readStored() !== null;
  }

  getRefreshToken(): string | null {
    return this.readStored()?.refreshToken ?? null;
  }

  // Returns a usable access token, refreshing first if it is missing or about to expire.
  async getAccessToken(): Promise<string | null> {
    if (this.accessToken && this.accessTokenExpiresAt - this.now() > 30_000) return this.accessToken;
    if (!this.hasRefreshToken()) return null;
    return (await this.refresh()) ? this.accessToken : null;
  }

  signIn(auth: AuthResponse): void {
    this.accessToken = auth.accessToken;
    this.accessTokenExpiresAt = Date.parse(auth.accessTokenExpiresAt);
    const stored: StoredSession = { refreshToken: auth.refreshToken, refreshTokenExpiresAt: auth.refreshTokenExpiresAt };
    this.storage.setItem(SESSION_STORAGE_KEY, JSON.stringify(stored));
    this.set({ status: 'signedIn', user: auth.user });
  }

  updateUser(user: CurrentUser): void {
    if (this.snapshot.status === 'signedIn') this.set({ status: 'signedIn', user });
  }

  signOut(): void {
    this.accessToken = null;
    this.accessTokenExpiresAt = 0;
    this.storage.setItem(SESSION_STORAGE_KEY, '');
    this.set({ status: 'signedOut', user: null });
  }

  // Single-flight within this tab, and one-at-a-time across tabs. Refresh tokens rotate, so two
  // refreshes with the same token would look like token theft and end the session.
  refresh(): Promise<boolean> {
    this.inflightRefresh ??= this.lock(() => this.doRefresh()).finally(() => {
      this.inflightRefresh = null;
    });
    return this.inflightRefresh;
  }

  private async doRefresh(): Promise<boolean> {
    // Read inside the lock: another tab may have just rotated the token and saved the new one.
    const stored = this.readStored();
    if (!stored) {
      this.signOut();
      return false;
    }
    try {
      const auth = await this.refreshCall(stored.refreshToken);
      if (!auth) {
        this.signOut();
        return false;
      }
      this.signIn(auth);
      return true;
    } catch {
      // Network problem: keep the stored session so the user is not signed out while offline.
      if (this.snapshot.status === 'loading') this.set({ status: 'signedOut', user: null });
      return false;
    }
  }

  private readStored(): StoredSession | null {
    const raw = this.storage.getItem(SESSION_STORAGE_KEY);
    if (!raw) return null;
    try {
      const parsed = JSON.parse(raw) as Partial<StoredSession>;
      if (typeof parsed.refreshToken !== 'string' || typeof parsed.refreshTokenExpiresAt !== 'string') return null;
      if (Date.parse(parsed.refreshTokenExpiresAt) <= this.now()) return null;
      return { refreshToken: parsed.refreshToken, refreshTokenExpiresAt: parsed.refreshTokenExpiresAt };
    } catch {
      return null;
    }
  }

  private set(snapshot: SessionSnapshot): void {
    this.snapshot = snapshot;
    for (const listener of this.listeners) listener();
  }
}
