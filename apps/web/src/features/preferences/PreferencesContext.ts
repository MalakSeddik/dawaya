import { createContext } from 'react';

import type { Language, Preferences } from './preferences';

export interface PreferencesContextValue {
  preferences: Preferences;
  setLanguage: (language: Language) => void;
  setLargeText: (enabled: boolean) => void;
  setHighContrast: (enabled: boolean) => void;
}

export const PreferencesContext = createContext<PreferencesContextValue | null>(null);
