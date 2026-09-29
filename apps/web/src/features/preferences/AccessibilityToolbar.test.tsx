import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { createMemoryStorage } from '@/lib/platform/storage';
import { renderApp } from '@/test/renderApp';

import { PREFERENCES_STORAGE_KEY } from './preferences';

// BR-33: large text and high contrast are available from the first screen and persist.
describe('accessibility toolbar on the first screen', () => {
  it('starts in Arabic, right-to-left', () => {
    renderApp('/');
    expect(document.documentElement.dir).toBe('rtl');
    expect(screen.getByRole('heading', { level: 1, name: 'أهلاً وسهلاً في دوايا' })).toBeInTheDocument();
  });

  it('switches to English and flips the layout to left-to-right', async () => {
    const user = userEvent.setup();
    renderApp('/');

    await user.click(screen.getByRole('button', { name: 'Switch to English' }));

    expect(document.documentElement.dir).toBe('ltr');
    expect(document.documentElement.lang).toBe('en');
    expect(await screen.findByRole('heading', { level: 1, name: 'Welcome to Dawaya' })).toBeInTheDocument();
  });

  it('toggles large text and high contrast and saves them', async () => {
    const user = userEvent.setup();
    const storage = createMemoryStorage();
    renderApp('/', storage);

    const largeText = screen.getByRole('button', { name: 'كلام كبير' });
    const highContrast = screen.getByRole('button', { name: 'ألوان واضحة' });
    await user.click(largeText);
    await user.click(highContrast);

    expect(largeText).toHaveAttribute('aria-pressed', 'true');
    expect(highContrast).toHaveAttribute('aria-pressed', 'true');
    expect(document.documentElement).toHaveAttribute('data-text-size', 'large');
    expect(document.documentElement).toHaveAttribute('data-contrast', 'high');
    expect(JSON.parse(storage.getItem(PREFERENCES_STORAGE_KEY) ?? '{}')).toMatchObject({
      largeText: true,
      highContrast: true,
    });
  });

  it('restores saved preferences on the next visit', () => {
    const storage = createMemoryStorage({
      [PREFERENCES_STORAGE_KEY]: JSON.stringify({ language: 'en', largeText: true, highContrast: false }),
    });

    renderApp('/', storage);

    expect(document.documentElement.dir).toBe('ltr');
    expect(document.documentElement).toHaveAttribute('data-text-size', 'large');
    expect(screen.getByRole('button', { name: 'Large text' })).toHaveAttribute('aria-pressed', 'true');
  });
});
