import { screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';

import { renderApp } from '@/test/renderApp';

function mockFetch(status: number, body: string, contentType = 'application/json') {
  vi.stubGlobal(
    'fetch',
    vi.fn(() => Promise.resolve(new Response(body, { status, headers: { 'Content-Type': contentType } }))),
  );
}

describe('settings: server status', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('shows the API as working when health is Healthy', async () => {
    mockFetch(200, JSON.stringify({ status: 'Healthy', checks: { database: 'Healthy' }, checkedAt: '2026-09-29T10:00:00Z' }));
    renderApp('/settings');
    expect(await screen.findByText('شغّال')).toBeInTheDocument();
  });

  it('shows "not available" when the API reports 503 Unhealthy', async () => {
    mockFetch(503, JSON.stringify({ status: 'Unhealthy', checks: { database: 'Unhealthy' }, checkedAt: '2026-09-29T10:00:00Z' }));
    renderApp('/settings');
    expect(await screen.findByText('مش متاح دلوقتي')).toBeInTheDocument();
  });

  it('shows "no connection" (never a raw key) when the API cannot be reached', async () => {
    mockFetch(500, 'proxy error', 'text/plain');
    renderApp('/settings');
    expect(await screen.findByText('مفيش اتصال')).toBeInTheDocument();
    expect(screen.queryByText(/settings\.status/)).not.toBeInTheDocument();
  });
});
