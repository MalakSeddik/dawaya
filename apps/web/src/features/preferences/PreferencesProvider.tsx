import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';

import { browserStorage, type KeyValueStorage } from '@/lib/platform/storage';

import { PreferencesContext, type PreferencesContextValue } from './PreferencesContext';
import { applyPreferencesToDocument, loadPreferences, savePreferences, type Preferences } from './preferences';

interface PreferencesProviderProps {
  children: ReactNode;
  storage?: KeyValueStorage;
}

// Device-level preferences (language, large text, high contrast). They work for guests from the
// first screen (BR-33); syncing them to the account comes with sign-in in Slice 1.
export function PreferencesProvider({ children, storage = browserStorage }: PreferencesProviderProps) {
  const { i18n } = useTranslation();
  const [preferences, setPreferences] = useState<Preferences>(() => loadPreferences(storage));

  useEffect(() => {
    applyPreferencesToDocument(preferences);
    savePreferences(storage, preferences);
    if (i18n.language !== preferences.language) void i18n.changeLanguage(preferences.language);
  }, [preferences, storage, i18n]);

  const update = useCallback((patch: Partial<Preferences>) => {
    setPreferences((current) => ({ ...current, ...patch }));
  }, []);

  const value = useMemo<PreferencesContextValue>(
    () => ({
      preferences,
      setLanguage: (language) => {
        update({ language });
      },
      setLargeText: (largeText) => {
        update({ largeText });
      },
      setHighContrast: (highContrast) => {
        update({ highContrast });
      },
    }),
    [preferences, update],
  );

  return <PreferencesContext value={value}>{children}</PreferencesContext>;
}
