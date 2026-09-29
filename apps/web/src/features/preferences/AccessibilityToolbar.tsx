import { ALargeSmall, Contrast, Languages } from 'lucide-react';
import { useTranslation } from 'react-i18next';

import { Button } from '@/components/ui/button';

import { usePreferences } from './usePreferences';

// Shown in the header of every screen, so these are reachable from the very first screen (BR-33).
export function AccessibilityToolbar() {
  const { t } = useTranslation();
  const { preferences, setLanguage, setLargeText, setHighContrast } = usePreferences();
  const otherLanguage = preferences.language === 'ar' ? 'en' : 'ar';

  return (
    <div role="group" aria-label={t('preferences.groupLabel')} className="flex items-center gap-1">
      <Button
        variant="ghost"
        size="touch"
        lang={otherLanguage}
        onClick={() => {
          setLanguage(otherLanguage);
        }}
        aria-label={t('preferences.switchLanguage')}
      >
        <Languages aria-hidden="true" />
        <span className="hidden sm:inline">{t('preferences.otherLanguageName')}</span>
      </Button>
      <Button
        variant="ghost"
        size="touch"
        aria-pressed={preferences.largeText}
        onClick={() => {
          setLargeText(!preferences.largeText);
        }}
        aria-label={t('preferences.largeText')}
        title={t('preferences.largeText')}
      >
        <ALargeSmall aria-hidden="true" />
      </Button>
      <Button
        variant="ghost"
        size="touch"
        aria-pressed={preferences.highContrast}
        onClick={() => {
          setHighContrast(!preferences.highContrast);
        }}
        aria-label={t('preferences.highContrast')}
        title={t('preferences.highContrast')}
      >
        <Contrast aria-hidden="true" />
      </Button>
    </div>
  );
}
