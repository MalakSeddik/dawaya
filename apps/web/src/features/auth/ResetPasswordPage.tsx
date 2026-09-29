import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';
import type { z } from 'zod';

import { ErrorAlert } from '@/components/ErrorAlert';
import { FormField } from '@/components/FormField';
import { PageHeader } from '@/components/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

import { resetPasswordSchema } from './schemas';
import { useResetPassword } from './useAuthActions';

// The emailed link is /reset-password#uid=...&token=... . The fragment never reaches any server log.
function readResetLink(): { userId: string; token: string } | null {
  const params = new URLSearchParams(window.location.hash.slice(1));
  const userId = params.get('uid');
  const token = params.get('token');
  return userId && token ? { userId, token } : null;
}

export function ResetPasswordPage() {
  const { t } = useTranslation();
  const [link] = useState(readResetLink);
  const reset = useResetPassword();
  const schema = resetPasswordSchema(t);
  const form = useForm<z.infer<typeof schema>>({ resolver: zodResolver(schema), defaultValues: { password: '', confirm: '' } });
  const { errors, isSubmitting } = form.formState;

  const onSubmit = form.handleSubmit(async (values) => {
    if (!link) return;
    try {
      await reset.mutateAsync({ userId: link.userId, token: link.token, newPassword: values.password });
      // Remove the token from the address bar and history.
      window.history.replaceState(null, '', window.location.pathname);
    } catch {
      // Shown by <ErrorAlert>
    }
  });

  if (!link) {
    return (
      <div className="mx-auto max-w-md">
        <PageHeader title={t('auth.resetTitle')} />
        <p role="alert">{t('errors.invalid_reset_link')}</p>
        <Link to="/forgot-password" className="mt-4 inline-flex min-h-touch items-center font-semibold text-primary underline">
          {t('auth.sendResetLink')}
        </Link>
      </div>
    );
  }

  if (reset.isSuccess) {
    return (
      <div className="mx-auto flex max-w-md flex-col items-center gap-4 text-center">
        <p role="status" className="text-lg">
          {t('auth.resetDone')}
        </p>
        <Button asChild size="lg">
          <Link to="/login">{t('auth.signIn')}</Link>
        </Button>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-md">
      <PageHeader title={t('auth.resetTitle')} />
      <form noValidate onSubmit={(e) => void onSubmit(e)} className="flex flex-col gap-4">
        <ErrorAlert error={reset.error} />
        <FormField label={t('auth.newPassword')} error={errors.password?.message}>
          <Input type="password" autoComplete="new-password" dir="ltr" {...form.register('password')} />
        </FormField>
        <FormField label={t('auth.confirmPassword')} error={errors.confirm?.message}>
          <Input type="password" autoComplete="new-password" dir="ltr" {...form.register('confirm')} />
        </FormField>
        <Button type="submit" size="lg" disabled={isSubmitting}>
          {t('auth.savePassword')}
        </Button>
      </form>
    </div>
  );
}
