import { useCallback, useSyncExternalStore } from 'react';

import { useApi } from '@/lib/ApiContext';
import type { KeyValueStorage } from '@/lib/platform/storage';

import { useProfiles, type Profile } from './useProfiles';

export const ACTIVE_PROFILE_STORAGE_KEY = 'dawaya.activeProfileId';

// Which family member the app is currently showing, remembered per device.
const listeners = new Set<() => void>();

function read(storage: KeyValueStorage): number | null {
  const value = Number(storage.getItem(ACTIVE_PROFILE_STORAGE_KEY));
  return Number.isInteger(value) && value > 0 ? value : null;
}

export function useActiveProfile(): {
  activeProfile: Profile | undefined;
  profiles: Profile[] | undefined;
  setActiveProfileId: (id: number) => void;
} {
  const { storage } = useApi();
  const { data: profiles } = useProfiles();
  const storedId = useSyncExternalStore(
    (listener) => {
      listeners.add(listener);
      return () => listeners.delete(listener);
    },
    () => read(storage),
  );

  const setActiveProfileId = useCallback(
    (id: number) => {
      storage.setItem(ACTIVE_PROFILE_STORAGE_KEY, String(id));
      for (const listener of listeners) listener();
    },
    [storage],
  );

  // Falls back to "Me" when nothing is chosen or the chosen profile was deleted.
  const activeProfile = profiles?.find((p) => p.id === storedId) ?? profiles?.find((p) => p.isSelf) ?? profiles?.[0];
  return { activeProfile, profiles, setActiveProfileId };
}
