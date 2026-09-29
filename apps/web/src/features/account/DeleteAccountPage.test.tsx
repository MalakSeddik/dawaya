import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';

import { authResponse, json, mockApi, problem, signedInStorage } from '@/test/mockApi';
import { renderApp } from '@/test/renderApp';

// BR-38: permanent delete needs the password and an explicit confirmation.
describe('delete account', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('requires the "I understand" confirmation', async () => {
    const user = userEvent.setup();
    const calls = mockApi({ 'POST /api/v1/auth/refresh': () => json(authResponse()) });
    renderApp('/settings/delete-account', signedInStorage());

    await user.type(await screen.findByLabelText('كلمة السر'), 'correct horse battery');
    await user.click(screen.getByRole('button', { name: 'امسح الحساب نهائياً' }));

    expect(await screen.findByText('لازم تأكد إنك فاهم إن المسح نهائي')).toBeInTheDocument();
    expect(calls.some((c) => c.path === '/api/v1/me/delete')).toBe(false);
  });

  it('shows a wrong-password error and keeps the user signed in', async () => {
    const user = userEvent.setup();
    mockApi({
      'POST /api/v1/auth/refresh': () => json(authResponse()),
      'POST /api/v1/me/delete': () => problem(400, 'wrong_password'),
    });
    const storage = signedInStorage();
    renderApp('/settings/delete-account', storage);

    await user.type(await screen.findByLabelText('كلمة السر'), 'nope nope');
    await user.click(screen.getByLabelText('فاهم إن المسح نهائي'));
    await user.click(screen.getByRole('button', { name: 'امسح الحساب نهائياً' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('كلمة السر غلط.');
  });

  it('deletes, signs out and returns home', async () => {
    const user = userEvent.setup();
    const calls = mockApi({
      'POST /api/v1/auth/refresh': () => json(authResponse()),
      'POST /api/v1/me/delete': () => new Response(null, { status: 204 }),
    });
    const storage = signedInStorage();
    const { router } = renderApp('/settings/delete-account', storage);

    await user.type(await screen.findByLabelText('كلمة السر'), 'correct horse battery');
    await user.click(screen.getByLabelText('فاهم إن المسح نهائي'));
    await user.click(screen.getByRole('button', { name: 'امسح الحساب نهائياً' }));

    expect(await screen.findByRole('heading', { level: 1, name: 'أهلاً وسهلاً في دوايا' })).toBeInTheDocument();
    expect(router.state.location.pathname).toBe('/');
    expect(calls.find((c) => c.path === '/api/v1/me/delete')?.body).toEqual({ password: 'correct horse battery' });
    expect(storage.getItem('dawaya.session')).toBe('');
  });
});
