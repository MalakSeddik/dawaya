import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

import { loadPreferences } from '@/features/preferences/preferences';
import { browserStorage } from '@/lib/platform/storage';

import arCommon from './locales/ar/common.json';
import enCommon from './locales/en/common.json';

export const resources = {
  ar: { common: arCommon },
  en: { common: enCommon },
} as const;

// No hard-coded UI strings: everything goes through these resource files (CLAUDE.md UI rules).
void i18n.use(initReactI18next).init({
  resources,
  lng: loadPreferences(browserStorage).language,
  fallbackLng: 'ar',
  defaultNS: 'common',
  ns: ['common'],
  interpolation: { escapeValue: false },
  returnNull: false,
});

export default i18n;
