import { z } from 'zod';

import type { KeyValueStorage } from '@/lib/platform/storage';

// Keep in sync with the inline script in index.html (a test checks this).
export const PREFERENCES_STORAGE_KEY = 'dawaya.preferences';

export const LANGUAGES = ['ar', 'en'] as const;
export type Language = (typeof LANGUAGES)[number];

const preferencesSchema = z.object({
  language: z.enum(LANGUAGES).catch('ar'),
  largeText: z.boolean().catch(false),
  highContrast: z.boolean().catch(false),
});

export type Preferences = z.infer<typeof preferencesSchema>;

// Arabic (Egyptian) is the default language (BR-17).
export const DEFAULT_PREFERENCES: Preferences = {
  language: 'ar',
  largeText: false,
  highContrast: false,
};

export function parsePreferences(raw: string | null): Preferences {
  if (!raw) return DEFAULT_PREFERENCES;
  try {
    const result = preferencesSchema.safeParse(JSON.parse(raw));
    return result.success ? result.data : DEFAULT_PREFERENCES;
  } catch {
    return DEFAULT_PREFERENCES;
  }
}

export function loadPreferences(storage: KeyValueStorage): Preferences {
  return parsePreferences(storage.getItem(PREFERENCES_STORAGE_KEY));
}

export function savePreferences(storage: KeyValueStorage, preferences: Preferences): void {
  storage.setItem(PREFERENCES_STORAGE_KEY, JSON.stringify(preferences));
}

export function directionOf(language: Language): 'rtl' | 'ltr' {
  return language === 'ar' ? 'rtl' : 'ltr';
}

// Large text and high contrast are CSS-variable themes switched by attributes on <html> (BR-33).
export function applyPreferencesToDocument(preferences: Preferences, root: HTMLElement = document.documentElement): void {
  root.lang = preferences.language;
  root.dir = directionOf(preferences.language);
  toggleAttribute(root, 'data-text-size', preferences.largeText ? 'large' : null);
  toggleAttribute(root, 'data-contrast', preferences.highContrast ? 'high' : null);
}

function toggleAttribute(root: HTMLElement, name: string, value: string | null): void {
  if (value === null) root.removeAttribute(name);
  else root.setAttribute(name, value);
}
