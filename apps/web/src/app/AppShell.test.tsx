import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { renderApp } from '@/test/renderApp';

describe('app shell', () => {
  it('has a skip link and labelled navigation with the main tabs', () => {
    renderApp('/');

    expect(screen.getByRole('link', { name: 'روح للمحتوى على طول' })).toHaveAttribute('href', '#main');
    const navs = screen.getAllByRole('navigation', { name: 'التنقل الرئيسي' });
    // Sidebar (desktop) and bottom tabs (phone); CSS shows one at a time.
    expect(navs).toHaveLength(2);
    const bottomTabs = navs[1];
    if (!bottomTabs) throw new Error('bottom tab bar missing');
    const labels = within(bottomTabs)
      .getAllByRole('link')
      .map((link) => link.textContent);
    expect(labels).toEqual(['الرئيسية', 'مسح الدوا', 'المواعيد', 'العيلة']);
  });

  it('marks the current tab and navigates in one tap (BR-35)', async () => {
    const user = userEvent.setup();
    const { router } = renderApp('/');
    const navs = screen.getAllByRole('navigation', { name: 'التنقل الرئيسي' });
    const bottomTabs = navs[1];
    if (!bottomTabs) throw new Error('bottom tab bar missing');

    expect(within(bottomTabs).getByRole('link', { name: 'الرئيسية' })).toHaveAttribute('aria-current', 'page');
    await user.click(within(bottomTabs).getByRole('link', { name: 'المواعيد' }));

    expect(router.state.location.pathname).toBe('/schedule');
    expect(screen.getByRole('heading', { level: 1, name: 'المواعيد' })).toBeInTheDocument();
  });

  it('shows the disclaimer on the home screen', () => {
    renderApp('/');
    expect(screen.getByRole('note')).toBeInTheDocument();
  });
});
