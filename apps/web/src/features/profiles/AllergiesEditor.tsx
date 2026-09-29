import { Plus, TriangleAlert, X } from 'lucide-react';
import { useEffect, useId, useState } from 'react';
import { useTranslation } from 'react-i18next';

import { ErrorAlert } from '@/components/ErrorAlert';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

import { allergyLabel } from './allergyLabel';
import { useAddAllergy, useIngredientSearch, useRemoveAllergy, type Profile } from './useProfiles';

function useDebounced(value: string, ms: number): string {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebounced(value);
    }, ms);
    return () => {
      clearTimeout(timer);
    };
  }, [value, ms]);
  return debounced;
}

// BR-07 / BR-25: known allergies. Picking a suggested ingredient links it, which lets the app show the
// red allergy warning on matching medicines later.
export function AllergiesEditor({ profile }: { profile: Profile }) {
  const { t, i18n } = useTranslation();
  const inputId = useId();
  const [text, setText] = useState('');
  const term = useDebounced(text, 250);
  const suggestions = useIngredientSearch(term);
  const add = useAddAllergy(profile.id);
  const remove = useRemoveAllergy(profile.id);

  const addAllergy = async (body: { ingredientId?: number; allergyText?: string }) => {
    try {
      await add.mutateAsync({ ingredientId: body.ingredientId ?? null, allergyText: body.allergyText ?? null });
      setText('');
    } catch {
      // Shown by <ErrorAlert>
    }
  };

  const typed = text.trim();

  return (
    <section aria-labelledby="allergies-heading" className="mt-10 flex flex-col gap-4">
      <h2 id="allergies-heading" className="flex items-center gap-2 text-xl font-bold">
        <TriangleAlert aria-hidden="true" className="size-6 text-destructive" />
        {t('allergies.title')}
      </h2>

      {profile.allergies.length === 0 ? (
        <p className="text-muted-foreground">{t('allergies.none')}</p>
      ) : (
        <ul className="flex flex-col gap-2">
          {profile.allergies.map((allergy) => {
            const label = allergyLabel(allergy, i18n.language);
            return (
              <li
                key={allergy.id}
                className="flex items-center justify-between gap-2 rounded-md border-(length:--border-width) border-destructive bg-card ps-3"
              >
                <span className="font-semibold">
                  {label}
                  {allergy.ingredientId ? null : (
                    <span className="ms-2 text-sm font-normal text-muted-foreground">({t('allergies.notLinked')})</span>
                  )}
                </span>
                <Button
                  variant="ghost"
                  size="touch"
                  aria-label={t('allergies.remove', { name: label })}
                  disabled={remove.isPending}
                  onClick={() => {
                    remove.mutate(allergy.id);
                  }}
                >
                  <X aria-hidden="true" />
                </Button>
              </li>
            );
          })}
        </ul>
      )}

      <ErrorAlert error={add.error ?? remove.error} />

      <div className="flex flex-col gap-2">
        <label htmlFor={inputId} className="font-semibold">
          {t('allergies.add')}
        </label>
        <p id={`${inputId}-hint`} className="text-sm text-muted-foreground">
          {t('allergies.addHint')}
        </p>
        <Input
          id={inputId}
          value={text}
          aria-describedby={`${inputId}-hint`}
          autoComplete="off"
          onChange={(e) => {
            setText(e.target.value);
          }}
        />
        {typed.length >= 2 && suggestions.data && suggestions.data.length > 0 ? (
          <ul aria-label={t('allergies.suggestions')} className="flex flex-col gap-1">
            {suggestions.data.map((ingredient) => (
              <li key={ingredient.id}>
                <Button
                  variant="outline"
                  className="w-full justify-start"
                  disabled={add.isPending}
                  onClick={() => void addAllergy({ ingredientId: ingredient.id })}
                >
                  <Plus aria-hidden="true" />
                  {i18n.language === 'ar' && ingredient.nameAr ? `${ingredient.nameAr} (${ingredient.nameEn})` : ingredient.nameEn}
                </Button>
              </li>
            ))}
          </ul>
        ) : null}
        {typed.length > 0 ? (
          <Button
            variant="secondary"
            className="self-start"
            disabled={add.isPending}
            onClick={() => void addAllergy({ allergyText: typed })}
          >
            <Plus aria-hidden="true" />
            {t('allergies.addAsTyped', { text: typed })}
          </Button>
        ) : null}
      </div>
    </section>
  );
}
