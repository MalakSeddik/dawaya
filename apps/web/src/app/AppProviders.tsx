import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState, type ReactNode } from 'react';
import { I18nextProvider } from 'react-i18next';

import { PreferencesProvider } from '@/features/preferences/PreferencesProvider';
import i18n from '@/i18n';
import type { KeyValueStorage } from '@/lib/platform/storage';

interface AppProvidersProps {
  children: ReactNode;
  storage?: KeyValueStorage;
}

export function AppProviders({ children, storage }: AppProvidersProps) {
  const [queryClient] = useState(
    () => new QueryClient({ defaultOptions: { queries: { staleTime: 30_000, retry: 1 } } }),
  );
  return (
    <I18nextProvider i18n={i18n}>
      <QueryClientProvider client={queryClient}>
        <PreferencesProvider {...(storage ? { storage } : {})}>{children}</PreferencesProvider>
      </QueryClientProvider>
    </I18nextProvider>
  );
}
