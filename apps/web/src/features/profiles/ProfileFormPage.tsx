import { zodResolver } from '@hookform/resolvers/zod';
import type { TFunction } from 'i18next';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams } from 'react-router';
import { z } from 'zod';

import { ErrorAlert } from '@/components/ErrorAlert';
import { FormField } from '@/components/FormField';
import { PageHeader } from '@/components/PageHeader';
import { SignInPrompt } from '@/components/SignInPrompt';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useSession } from '@/features/auth/AuthContext';

import { AllergiesEditor } from './AllergiesEditor';
import {
  AGE_GROUPS,
  useCreateProfile,
  useDeleteProfile,
  useProfile,
  useUpdateProfile,
  type Profile,
  type SaveProfile,
} from './useProfiles';

function profileSchema(t: TFunction) {
  return z.object({
    displayName: z.string().trim().min(1, t('validation.required')).max(100, t('validation.tooLong')),
    relationship: z.string().trim().max(50, t('validation.tooLong')),
    ageGroup: z.enum(['', ...AGE_GROUPS]),
    isPregnant: z.boolean(),
    isBreastfeeding: z.boolean(),
  });
}

type ProfileFormValues = z.infer<ReturnType<typeof profileSchema>>;

function toRequest(values: ProfileFormValues): SaveProfile {
  return {
    displayName: values.displayName,
    relationship: values.relationship || null,
    ageGroup: values.ageGroup || null,
    isPregnant: values.isPregnant,
    isBreastfeeding: values.isBreastfeeding,
  };
}

// /profiles/new and /profiles/:id
export function ProfileFormPage() {
  const { t } = useTranslation();
  const { status } = useSession();
  const { id } = useParams();
  const profileId = id ? Number(id) : null;

  if (status === 'signedOut') {
    return (
      <>
        <PageHeader title={t('profiles.title')} />
        <SignInPrompt message={t('profiles.signInToManage')} />
      </>
    );
  }
  return profileId === null ? <NewProfile /> : <EditProfile id={profileId} />;
}

function NewProfile() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const create = useCreateProfile();
  return (
    <>
      <PageHeader title={t('profiles.addTitle')} />
      <ProfileForm
        error={create.error}
        submitLabel={t('profiles.saveAndAddAllergies')}
        onSubmit={async (values) => {
          const profile = await create.mutateAsync(toRequest(values));
          // Straight to the edit page, where allergies can be added.
          await navigate(`/profiles/${profile.id}`, { replace: true });
        }}
      />
    </>
  );
}

function EditProfile({ id }: { id: number }) {
  const { t } = useTranslation();
  const { data: profile, error, isPending } = useProfile(id);
  const update = useUpdateProfile(id);
  const [saved, setSaved] = useState(false);

  if (isPending) return <p role="status">{t('common.loading')}</p>;
  if (!profile) return <ErrorAlert error={error} />;

  return (
    <>
      <PageHeader title={profile.isSelf ? t('profiles.meLabel', { name: profile.displayName }) : profile.displayName} />
      <ProfileForm
        key={profile.id}
        profile={profile}
        error={update.error}
        submitLabel={t('common.save')}
        onSubmit={async (values) => {
          setSaved(false);
          await update.mutateAsync(toRequest(values));
          setSaved(true);
        }}
      />
      {saved ? (
        <p role="status" className="mt-3 font-semibold text-primary">
          {t('common.saved')}
        </p>
      ) : null}
      <AllergiesEditor profile={profile} />
      {profile.isSelf ? null : <DeleteProfile profile={profile} />}
    </>
  );
}

interface ProfileFormProps {
  profile?: Profile;
  error: unknown;
  submitLabel: string;
  onSubmit: (values: ProfileFormValues) => Promise<void>;
}

function ProfileForm({ profile, error, submitLabel, onSubmit }: ProfileFormProps) {
  const { t } = useTranslation();
  const schema = profileSchema(t);
  const form = useForm<ProfileFormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      displayName: profile?.displayName ?? '',
      relationship: profile?.relationship ?? '',
      ageGroup: profile?.ageGroup ?? '',
      isPregnant: profile?.isPregnant ?? false,
      isBreastfeeding: profile?.isBreastfeeding ?? false,
    },
  });
  const { errors, isSubmitting } = form.formState;

  const submit = form.handleSubmit(async (values) => {
    try {
      await onSubmit(values);
    } catch {
      // Shown by <ErrorAlert>
    }
  });

  return (
    <form noValidate onSubmit={(e) => void submit(e)} className="flex flex-col gap-4">
      <ErrorAlert error={error} />
      <FormField label={t('profiles.name')} error={errors.displayName?.message}>
        <Input autoComplete="off" {...form.register('displayName')} />
      </FormField>
      <FormField label={t('profiles.relationship')} hint={t('profiles.relationshipHint')} error={errors.relationship?.message}>
        <Input autoComplete="off" {...form.register('relationship')} />
      </FormField>
      <FormField label={t('profiles.ageGroup')}>
        <select
          className="min-h-touch rounded-md border-(length:--border-width) bg-card px-3 text-base"
          {...form.register('ageGroup')}
        >
          <option value="">{t('profiles.ageGroupNotSet')}</option>
          {AGE_GROUPS.map((group) => (
            <option key={group} value={group}>
              {t(`ageGroups.${group}.label`)} ({t(`ageGroups.${group}.range`)})
            </option>
          ))}
        </select>
      </FormField>

      <fieldset className="flex flex-col gap-2 rounded-lg border-(length:--border-width) p-4">
        <legend className="px-1 font-semibold">{t('profiles.specialCases')}</legend>
        {/* BR-07: these only change which warnings are highlighted, never the dose. */}
        <p className="text-sm text-muted-foreground">{t('profiles.warningsOnlyHint')}</p>
        <label className="flex min-h-touch cursor-pointer items-center gap-3">
          <input type="checkbox" className="size-6 accent-(--primary)" {...form.register('isPregnant')} />
          {t('profiles.pregnant')}
        </label>
        <label className="flex min-h-touch cursor-pointer items-center gap-3">
          <input type="checkbox" className="size-6 accent-(--primary)" {...form.register('isBreastfeeding')} />
          {t('profiles.breastfeeding')}
        </label>
      </fieldset>

      <Button type="submit" size="lg" disabled={isSubmitting}>
        {submitLabel}
      </Button>
    </form>
  );
}

function DeleteProfile({ profile }: { profile: Profile }) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const remove = useDeleteProfile(profile.id);
  const [confirming, setConfirming] = useState(false);

  return (
    <section aria-labelledby="delete-profile" className="mt-10 flex flex-col gap-3 border-t-(length:--border-width) pt-6">
      <h2 id="delete-profile" className="text-lg font-bold">
        {t('profiles.deleteTitle')}
      </h2>
      <ErrorAlert error={remove.error} />
      {confirming ? (
        <div className="flex flex-col gap-3" role="group" aria-label={t('profiles.deleteTitle')}>
          <p className="font-semibold">{t('profiles.deleteConfirm', { name: profile.displayName })}</p>
          <div className="flex flex-wrap gap-2">
            <Button
              variant="destructive"
              disabled={remove.isPending}
              onClick={() => {
                void remove.mutateAsync().then(() => navigate('/profiles', { replace: true }), () => undefined);
              }}
            >
              {t('profiles.deleteYes')}
            </Button>
            <Button
              variant="outline"
              onClick={() => {
                setConfirming(false);
              }}
            >
              {t('common.cancel')}
            </Button>
          </div>
        </div>
      ) : (
        <Button
          variant="outline"
          className="self-start text-destructive"
          onClick={() => {
            setConfirming(true);
          }}
        >
          {t('profiles.delete', { name: profile.displayName })}
        </Button>
      )}
    </section>
  );
}
