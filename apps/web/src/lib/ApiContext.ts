import { createContext, use } from 'react';

import type { ApiContext } from './api';

export const ApiReactContext = createContext<ApiContext | null>(null);

export function useApi(): ApiContext {
  const context = use(ApiReactContext);
  if (!context) throw new Error('useApi must be used inside <AppProviders>');
  return context;
}
