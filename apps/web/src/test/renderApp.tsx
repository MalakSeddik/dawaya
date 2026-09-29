import { render } from '@testing-library/react';
import { createMemoryRouter, RouterProvider } from 'react-router';

import { AppProviders } from '@/app/AppProviders';
import { routes } from '@/app/routes';
import { createMemoryStorage, type KeyValueStorage } from '@/lib/platform/storage';

export function renderApp(path = '/', storage: KeyValueStorage = createMemoryStorage()) {
  const router = createMemoryRouter(routes, { initialEntries: [path] });
  const result = render(
    <AppProviders storage={storage}>
      <RouterProvider router={router} />
    </AppProviders>,
  );
  return { ...result, router, storage };
}
