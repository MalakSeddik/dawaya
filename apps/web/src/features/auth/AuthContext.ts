import { useSyncExternalStore } from 'react';

import { useApi } from '@/lib/ApiContext';

import type { SessionSnapshot } from './session';

export function useSession(): SessionSnapshot {
  const { session } = useApi();
  return useSyncExternalStore(session.subscribe, session.getSnapshot);
}
