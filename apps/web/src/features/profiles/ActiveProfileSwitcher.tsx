import { useId } from 'react';
import { useTranslation } from 'react-i18next';

import { useActiveProfile } from './useActiveProfile';

// "Who am I managing right now?" Shown on the home screen for signed-in users (BR-06).
export function ActiveProfileSwitcher() {
  const { t } = useTranslation();
  const id = useId();
  const { activeProfile, profiles, setActiveProfileId } = useActiveProfile();
  if (!profiles || profiles.length === 0 || !activeProfile) return null;

  return (
    <div className="mb-6 flex flex-col gap-1.5 rounded-lg border-(length:--border-width) bg-card p-4">
      <label htmlFor={id} className="font-semibold">
        {t('profiles.showingFor')}
      </label>
      <select
        id={id}
        value={activeProfile.id}
        onChange={(e) => {
          setActiveProfileId(Number(e.target.value));
        }}
        className="min-h-touch rounded-md border-(length:--border-width) bg-card px-3 text-lg font-bold"
      >
        {profiles.map((p) => (
          <option key={p.id} value={p.id}>
            {p.isSelf ? t('profiles.meLabel', { name: p.displayName }) : p.displayName}
          </option>
        ))}
      </select>
    </div>
  );
}
