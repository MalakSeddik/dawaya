import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';

import { PREFERENCES_STORAGE_KEY } from '@/features/preferences/preferences';
import { createMemoryStorage } from '@/lib/platform/storage';
import { authResponse, json, mockApi, profile } from '@/test/mockApi';
import { renderApp } from '@/test/renderApp';

describe('create account', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('keeps large text and high contrast that were turned on as a guest (BR-33)', async () => {
    const user = userEvent.setup();
    const calls = mockApi({
      'POST /api/v1/auth/register': (call) => {
        const body = call.body as { largeText: boolean; highContrast: boolean };
        return json(authResponse({ largeText: body.largeText, highContrast: body.highContrast }));
      },
      'GET /api/v1/profiles': () => json([profile()]),
    });
    const storage = createMemoryStorage({
      [PREFERENCES_STORAGE_KEY]: JSON.stringify({ language: 'ar', largeText: true, highContrast: true }),
    });
    renderApp('/register', storage);

    await user.type(screen.getByLabelText('اسمك'), 'Malak');
    await user.type(screen.getByLabelText('الإيميل'), 'malak@example.test');
    await user.type(screen.getByLabelText('كلمة السر'), 'correct horse battery');
    await user.click(screen.getByRole('button', { name: 'إنشاء حساب' }));

    expect(await screen.findByRole('article', { name: 'Malak (أنا)' })).toBeInTheDocument();
    expect(calls.find((c) => c.path === '/api/v1/auth/register')?.body).toMatchObject({
      preferredLanguage: 'ar',
      largeText: true,
      highContrast: true,
    });
    await waitFor(() => {
      expect(document.documentElement).toHaveAttribute('data-text-size', 'large');
    });
    expect(document.documentElement).toHaveAttribute('data-contrast', 'high');
  });

  it('shows a clear message when the email already has an account', async () => {
    const user = userEvent.setup();
    mockApi({
      'POST /api/v1/auth/register': () =>
        new Response(JSON.stringify({ status: 409, code: 'email_taken' }), {
          status: 409,
          headers: { 'Content-Type': 'application/problem+json' },
        }),
    });
    renderApp('/register');

    await user.type(screen.getByLabelText('اسمك'), 'Malak');
    await user.type(screen.getByLabelText('الإيميل'), 'malak@example.test');
    await user.type(screen.getByLabelText('كلمة السر'), 'correct horse battery');
    await user.click(screen.getByRole('button', { name: 'إنشاء حساب' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('فيه حساب بالإيميل ده بالفعل');
  });
});
