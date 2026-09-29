import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';

import { mockApi } from '@/test/mockApi';
import { renderApp } from '@/test/renderApp';

describe('reset password', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    window.history.replaceState(null, '', '/');
  });

  it('reads the id and token from the link fragment, resets, and removes the token from the address', async () => {
    const user = userEvent.setup();
    window.history.replaceState(null, '', '/reset-password#uid=user-1&token=abc%2B123');
    const calls = mockApi({ 'POST /api/v1/auth/reset-password': () => new Response(null, { status: 204 }) });
    renderApp('/reset-password');

    await user.type(screen.getByLabelText('كلمة السر الجديدة'), 'a brand new password');
    await user.type(screen.getByLabelText('اكتب كلمة السر تاني'), 'a brand new password');
    await user.click(screen.getByRole('button', { name: 'احفظ كلمة السر' }));

    expect(await screen.findByRole('status')).toHaveTextContent('كلمة السر اتغيرت');
    expect(calls[0]?.body).toEqual({ userId: 'user-1', token: 'abc+123', newPassword: 'a brand new password' });
    expect(window.location.hash).toBe('');
  });

  it('checks that both passwords match', async () => {
    const user = userEvent.setup();
    window.history.replaceState(null, '', '/reset-password#uid=user-1&token=t');
    mockApi({});
    renderApp('/reset-password');

    await user.type(screen.getByLabelText('كلمة السر الجديدة'), 'a brand new password');
    await user.type(screen.getByLabelText('اكتب كلمة السر تاني'), 'something else');
    await user.click(screen.getByRole('button', { name: 'احفظ كلمة السر' }));

    expect(await screen.findByText('كلمتين السر مش زي بعض')).toBeInTheDocument();
  });

  it('explains when the link is incomplete', () => {
    mockApi({});
    renderApp('/reset-password');

    expect(screen.getByRole('alert')).toHaveTextContent('اللينك ده مش شغال أو انتهى');
  });
});
