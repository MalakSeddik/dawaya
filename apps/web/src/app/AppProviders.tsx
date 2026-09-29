import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useEffect, useState, type ReactNode } from 'react';
import { I18nextProvider } from 'react-i18next';

import { PreferencesProvider } from '@/features/preferences/PreferencesProvider';
import { PreferencesSync } from '@/features/preferences/PreferencesSync';
import i18n from '@/i18n';
import { createApiContext } from '@/lib/api';
import { ApiReactContext } from '@/lib/ApiContext';
import { browserStorage, type KeyValueStorage } from '@/lib/platform/storage';

interface AppProvidersProps {
  children: ReactNode;
  storage?: KeyValueStorage;
}

export function AppProviders({ children, storage = browserStorage }: AppProvidersProps) {
  const [queryClient] = useState(
    () => new QueryClient({ defaultOptions: { queries: { staleTime: 30_000, retry: 1 } } }),
  );
  const [apiContext] = useState(() => createApiContext(storage));

  // Restore a saved session on start-up (the access token is never stored, so refresh once).
  // If the server was unreachable, try again when the device is back online or the app is reopened.
  useEffect(() => {
    const { session } = apiContext;
    const restore = () => {
      if (session.getSnapshot().status !== 'signedIn' && session.hasRefreshToken()) void session.refresh();
    };
    restore();
    window.addEventListener('online', restore);
    window.addEventListener('focus', restore);
    return () => {
      window.removeEventListener('online', restore);
      window.removeEventListener('focus', restore);
    };
  }, [apiContext]);

  return (
    <I18nextProvider i18n={i18n}>
      <ApiReactContext value={apiContext}>
        <QueryClientProvider client={queryClient}>
          <PreferencesProvider storage={storage}>
            <PreferencesSync />
            {children}
          </PreferencesProvider>
        </QueryClientProvider>
      </ApiReactContext>
    </I18nextProvider>
  );
}
