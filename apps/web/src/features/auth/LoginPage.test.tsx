import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';

import { authResponse, json, mockApi, problem, profile } from '@/test/mockApi';
import { renderApp } from '@/test/renderApp';

describe('sign in', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('asks for the missing fields before calling the server', async () => {
    const user = userEvent.setup();
    const calls = mockApi({});
    renderApp('/login');

    await user.click(screen.getByRole('button', { name: 'تسجيل الدخول' }));

    expect(await screen.findAllByText('الخانة دي مطلوبة')).toHaveLength(2);
    expect(screen.getByLabelText('الإيميل')).toHaveAttribute('aria-invalid', 'true');
    expect(calls.filter((c) => c.path.startsWith('/api/v1/auth'))).toHaveLength(0);
  });

  it('shows a translated message for wrong credentials', async () => {
    const user = userEvent.setup();
    mockApi({ 'POST /api/v1/auth/login': () => problem(401, 'invalid_credentials') });
    renderApp('/login');

    await user.type(screen.getByLabelText('الإيميل'), 'malak@example.test');
    await user.type(screen.getByLabelText('كلمة السر'), 'wrong password');
    await user.click(screen.getByRole('button', { name: 'تسجيل الدخول' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('الإيميل أو كلمة السر غلط.');
  });

  it('signs in and goes home, where the active profile switcher appears', async () => {
    const user = userEvent.setup();
    const calls = mockApi({
      'POST /api/v1/auth/login': () => json(authResponse()),
      'GET /api/v1/profiles': () => json([profile(), profile({ id: 2, displayName: 'Mom', isSelf: false })]),
    });
    const { router } = renderApp('/login');

    await user.type(screen.getByLabelText('الإيميل'), 'malak@example.test');
    await user.type(screen.getByLabelText('كلمة السر'), 'correct horse battery');
    await user.click(screen.getByRole('button', { name: 'تسجيل الدخول' }));

    expect(await screen.findByLabelText('بتتابع أدوية مين دلوقتي؟')).toHaveDisplayValue('Malak (أنا)');
    expect(router.state.location.pathname).toBe('/');
    expect(calls.find((c) => c.path === '/api/v1/auth/login')?.body).toEqual({
      email: 'malak@example.test',
      password: 'correct horse battery',
    });
  });
});
