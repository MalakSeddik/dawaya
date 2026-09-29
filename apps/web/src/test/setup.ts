import '@testing-library/jest-dom/vitest';

import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

import i18n from '@/i18n';

afterEach(async () => {
  cleanup();
  window.localStorage.clear();
  const html = document.documentElement;
  html.removeAttribute('data-text-size');
  html.removeAttribute('data-contrast');
  html.lang = 'ar';
  html.dir = 'rtl';
  await i18n.changeLanguage('ar');
});
