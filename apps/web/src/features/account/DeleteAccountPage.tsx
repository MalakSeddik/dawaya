import { zodResolver } from '@hookform/resolvers/zod';
import { TriangleAlert } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { Link, useNavigate } from 'react-router';
import { z } from 'zod';

import { ErrorAlert } from '@/components/ErrorAlert';
import { FormField } from '@/components/FormField';
import { PageHeader } from '@/components/PageHeader';
import { SignInPrompt } from '@/components/SignInPrompt';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useSession } from '@/features/auth/AuthContext';

import { useDeleteAccount } from './useAccount';

// BR-38: permanently delete everything. Needs the password and an explicit "I understand".
export function DeleteAccountPage() {
  const { t } = useTranslation();
  const { status } = useSession();
  const navigate = useNavigate();
  const deleteAccount = useDeleteAccount();
  const schema = z.object({
    password: z.string().min(1, t('validation.required')),
    understood: z.boolean().refine((v) => v, t('account.mustConfirm')),
  });
  const form = useForm<{ password: string; understood: boolean }>({
    resolver: zodResolver(schema),
    defaultValues: { password: '', understood: false },
  });
  const { errors, isSubmitting } = form.formState;

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await deleteAccount.mutateAsync(values.password);
      await navigate('/', { replace: true, state: { accountDeleted: true } });
    } catch {
      // Shown by <ErrorAlert>
    }
  });

  if (status === 'signedOut') {
    return (
      <>
        <PageHeader title={t('account.deleteTitle')} />
        <SignInPrompt message={t('account.guestMessage')} />
      </>
    );
  }

  return (
    <div className="mx-auto max-w-md">
      <PageHeader title={t('account.deleteTitle')} />
      <div className="mb-6 flex items-start gap-3 rounded-lg border-(length:--border-width) border-destructive p-4 text-destructive">
        <TriangleAlert aria-hidden="true" className="mt-0.5 size-6 shrink-0" />
        <div className="flex flex-col gap-2">
          <p className="font-bold">{t('account.deleteWarning')}</p>
          <p>{t('account.deleteExportHint')}</p>
        </div>
      </div>
      <form noValidate onSubmit={(e) => void onSubmit(e)} className="flex flex-col gap-4">
        <ErrorAlert error={deleteAccount.error} />
        <FormField label={t('auth.password')} error={errors.password?.message}>
          <Input type="password" autoComplete="current-password" dir="ltr" {...form.register('password')} />
        </FormField>
        <div className="flex flex-col gap-1">
          <label className="flex min-h-touch cursor-pointer items-center gap-3 font-semibold">
            <input
              type="checkbox"
              className="size-6 accent-(--destructive)"
              aria-invalid={errors.understood ? true : undefined}
              {...form.register('understood')}
            />
            {t('account.deleteUnderstand')}
          </label>
          {errors.understood ? (
            <p className="text-sm font-semibold text-destructive">{errors.understood.message}</p>
          ) : null}
        </div>
        <Button type="submit" variant="destructive" size="lg" disabled={isSubmitting}>
          {t('account.deleteForever')}
        </Button>
        <Button asChild variant="outline">
          <Link to="/settings">{t('common.cancel')}</Link>
        </Button>
      </form>
    </div>
  );
}
