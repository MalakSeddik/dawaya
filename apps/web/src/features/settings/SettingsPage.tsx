import { useId } from 'react';
import { useTranslation } from 'react-i18next';

import { PageHeader } from '@/components/PageHeader';
import { Button } from '@/components/ui/button';
import { LANGUAGES } from '@/features/preferences/preferences';
import { usePreferences } from '@/features/preferences/usePreferences';
import { useApiHealth } from '@/features/system/useApiHealth';

const LANGUAGE_NAMES = { ar: 'العربية', en: 'English' } as const;

export function SettingsPage() {
  const { t } = useTranslation();
  const { preferences, setLanguage, setLargeText, setHighContrast } = usePreferences();
  const health = useApiHealth();

  const statusText = health.isPending
    ? t('settings.status.checking')
    : health.isError
      ? t('settings.status.offline')
      : t(`settings.status.${health.data.status}`);

  return (
    <>
      <PageHeader title={t('settings.title')} />

      <section aria-labelledby="display-heading" className="mb-8 flex flex-col gap-4">
        <h2 id="display-heading" className="text-xl font-bold">
          {t('preferences.title')}
        </h2>

        <fieldset className="flex flex-col gap-2">
          <legend className="mb-2 font-semibold">{t('preferences.language')}</legend>
          <div className="flex gap-2">
            {LANGUAGES.map((language) => (
              <Button
                key={language}
                lang={language}
                variant={preferences.language === language ? 'default' : 'outline'}
                aria-pressed={preferences.language === language}
                onClick={() => {
                  setLanguage(language);
                }}
              >
                {LANGUAGE_NAMES[language]}
              </Button>
            ))}
          </div>
        </fieldset>

        <ToggleRow
          label={t('preferences.largeText')}
          hint={t('preferences.largeTextHint')}
          checked={preferences.largeText}
          onChange={setLargeText}
        />
        <ToggleRow
          label={t('preferences.highContrast')}
          hint={t('preferences.highContrastHint')}
          checked={preferences.highContrast}
          onChange={setHighContrast}
        />
      </section>

      <section aria-labelledby="about-heading" className="flex flex-col gap-2">
        <h2 id="about-heading" className="text-xl font-bold">
          {t('settings.about')}
        </h2>
        <p>
          {t('settings.serverStatus')}: <span role="status">{statusText}</span>
        </p>
      </section>
    </>
  );
}

interface ToggleRowProps {
  label: string;
  hint: string;
  checked: boolean;
  onChange: (value: boolean) => void;
}

function ToggleRow({ label, hint, checked, onChange }: ToggleRowProps) {
  const id = useId();
  return (
    <div className="flex min-h-touch items-center justify-between gap-4 rounded-lg border-(length:--border-width) bg-card p-4">
      <div className="flex flex-col">
        <label htmlFor={id} className="cursor-pointer font-semibold">
          {label}
        </label>
        <span id={`${id}-hint`} className="text-muted-foreground">
          {hint}
        </span>
      </div>
      <input
        id={id}
        type="checkbox"
        role="switch"
        aria-describedby={`${id}-hint`}
        checked={checked}
        onChange={(e) => {
          onChange(e.target.checked);
        }}
        className="size-6 shrink-0 cursor-pointer accent-(--primary)"
      />
    </div>
  );
}
