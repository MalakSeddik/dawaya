import { use } from 'react';

import { PreferencesContext, type PreferencesContextValue } from './PreferencesContext';

export function usePreferences(): PreferencesContextValue {
  const context = use(PreferencesContext);
  if (!context) throw new Error('usePreferences must be used inside <PreferencesProvider>');
  return context;
}
