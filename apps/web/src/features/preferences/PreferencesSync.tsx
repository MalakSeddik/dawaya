import { useEffect, useRef } from 'react';

import { useSession } from '@/features/auth/AuthContext';
import { useApi } from '@/lib/ApiContext';

import type { Preferences } from './preferences';
import { usePreferences } from './usePreferences';

// BR-33: preferences follow the signed-in user across devices.
// On sign-in the account's saved preferences are applied; after that, local changes are saved to the account.
export function PreferencesSync() {
  const { api } = useApi();
  const { status, user } = useSession();
  const { preferences, replacePreferences } = usePreferences();
  const lastSynced = useRef<{ userId: string; value: string } | null>(null);

  useEffect(() => {
    if (status !== 'signedIn' || !user) {
      lastSynced.current = null;
      return;
    }
    const fromServer: Preferences = {
      language: user.preferredLanguage === 'en' ? 'en' : 'ar',
      largeText: user.largeText,
      highContrast: user.highContrast,
    };

    if (lastSynced.current?.userId !== user.id) {
      // Just signed in: the account's preferences win.
      lastSynced.current = { userId: user.id, value: JSON.stringify(fromServer) };
      replacePreferences(fromServer);
      return;
    }

    const local = JSON.stringify(preferences);
    if (local === lastSynced.current.value) return;
    lastSynced.current = { userId: user.id, value: local };
    void api.PUT('/api/v1/me/preferences', {
      body: {
        preferredLanguage: preferences.language,
        largeText: preferences.largeText,
        highContrast: preferences.highContrast,
      },
    });
  }, [api, status, user, preferences, replacePreferences]);

  return null;
}
