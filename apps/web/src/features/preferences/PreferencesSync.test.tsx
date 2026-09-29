import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';

import { authResponse, json, mockApi, signedInStorage } from '@/test/mockApi';
import { renderApp } from '@/test/renderApp';

// BR-33: large text, high contrast and language follow the signed-in user across devices.
describe('preferences sync', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('applies the account preferences when the session starts', async () => {
    mockApi({
      'POST /api/v1/auth/refresh': () => json(authResponse({ preferredLanguage: 'en', largeText: true, highContrast: true })),
    });

    renderApp('/', signedInStorage());

    await waitFor(() => {
      expect(document.documentElement).toHaveAttribute('data-text-size', 'large');
    });
    expect(document.documentElement).toHaveAttribute('data-contrast', 'high');
    expect(document.documentElement.dir).toBe('ltr');
  });

  it('saves a change made on this device to the account', async () => {
    const user = userEvent.setup();
    const calls = mockApi({
      'POST /api/v1/auth/refresh': () => json(authResponse()),
      'PUT /api/v1/me/preferences': (call) => json({ ...authResponse().user, ...(call.body as object) }),
    });
    renderApp('/', signedInStorage());
    await screen.findByRole('heading', { level: 1 });
    await waitFor(() => {
      expect(calls.some((c) => c.path === '/api/v1/auth/refresh')).toBe(true);
    });

    await user.click(screen.getByRole('button', { name: 'كلام كبير' }));

    await waitFor(() => {
      expect(calls.find((c) => c.path === '/api/v1/me/preferences')?.body).toEqual({
        preferredLanguage: 'ar',
        largeText: true,
        highContrast: false,
      });
    });
  });

  it('does not call the server for guests', async () => {
    const user = userEvent.setup();
    const calls = mockApi({});
    renderApp('/');

    await user.click(screen.getByRole('button', { name: 'كلام كبير' }));

    expect(calls.some((c) => c.path === '/api/v1/me/preferences')).toBe(false);
  });
});
