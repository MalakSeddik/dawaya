// Small key-value storage interface (CLAUDE.md rule 5): the web app uses localStorage,
// a mobile app can swap in its own implementation (e.g. Capacitor Preferences).
export interface KeyValueStorage {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
}

// localStorage can throw (private mode, blocked site data), so every access is guarded.
export const browserStorage: KeyValueStorage = {
  getItem(key) {
    try {
      return window.localStorage.getItem(key);
    } catch {
      return null;
    }
  },
  setItem(key, value) {
    try {
      window.localStorage.setItem(key, value);
    } catch {
      // Preferences still apply for this session; they just won't persist.
    }
  },
};

export function createMemoryStorage(initial: Record<string, string> = {}): KeyValueStorage {
  const data = new Map(Object.entries(initial));
  return {
    getItem: (key) => data.get(key) ?? null,
    setItem: (key, value) => {
      data.set(key, value);
    },
  };
}
