import { describe, expect, it } from 'vitest';

import indexHtml from '../../../index.html?raw';

import {
  applyPreferencesToDocument,
  DEFAULT_PREFERENCES,
  parsePreferences,
  PREFERENCES_STORAGE_KEY,
} from './preferences';

describe('parsePreferences', () => {
  it('defaults to Arabic with normal text when nothing is saved', () => {
    expect(parsePreferences(null)).toEqual({ language: 'ar', largeText: false, highContrast: false });
  });

  it('reads saved values', () => {
    const raw = JSON.stringify({ language: 'en', largeText: true, highContrast: true });
    expect(parsePreferences(raw)).toEqual({ language: 'en', largeText: true, highContrast: true });
  });

  it('falls back per field for invalid values and to defaults for broken JSON', () => {
    expect(parsePreferences(JSON.stringify({ language: 'fr', largeText: 'yes', highContrast: true }))).toEqual({
      language: 'ar',
      largeText: false,
      highContrast: true,
    });
    expect(parsePreferences('{not json')).toEqual(DEFAULT_PREFERENCES);
  });
});

describe('applyPreferencesToDocument', () => {
  it('sets RTL for Arabic and LTR for English', () => {
    const el = document.createElement('html');
    applyPreferencesToDocument({ ...DEFAULT_PREFERENCES, language: 'ar' }, el);
    expect(el.dir).toBe('rtl');
    expect(el.lang).toBe('ar');

    applyPreferencesToDocument({ ...DEFAULT_PREFERENCES, language: 'en' }, el);
    expect(el.dir).toBe('ltr');
    expect(el.lang).toBe('en');
  });

  it('adds and removes the large-text and high-contrast attributes', () => {
    const el = document.createElement('html');
    applyPreferencesToDocument({ language: 'ar', largeText: true, highContrast: true }, el);
    expect(el.getAttribute('data-text-size')).toBe('large');
    expect(el.getAttribute('data-contrast')).toBe('high');

    applyPreferencesToDocument(DEFAULT_PREFERENCES, el);
    expect(el.hasAttribute('data-text-size')).toBe(false);
    expect(el.hasAttribute('data-contrast')).toBe(false);
  });
});

describe('index.html first-paint script', () => {
  it('reads the same storage key as the app', () => {
    expect(indexHtml).toContain(`localStorage.getItem('${PREFERENCES_STORAGE_KEY}')`);
  });
});
