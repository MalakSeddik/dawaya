import { zodResolver } from '@hookform/resolvers/zod';
import { MailCheck } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';
import type { z } from 'zod';

import { ErrorAlert } from '@/components/ErrorAlert';
import { FormField } from '@/components/FormField';
import { PageHeader } from '@/components/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

import { forgotPasswordSchema } from './schemas';
import { useForgotPassword } from './useAuthActions';

export function ForgotPasswordPage() {
  const { t } = useTranslation();
  const forgot = useForgotPassword();
  const schema = forgotPasswordSchema(t);
  const form = useForm<z.infer<typeof schema>>({ resolver: zodResolver(schema), defaultValues: { email: '' } });

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await forgot.mutateAsync(values);
    } catch {
      // Shown by <ErrorAlert>
    }
  });

  if (forgot.isSuccess) {
    return (
      <div className="mx-auto flex max-w-md flex-col items-center gap-4 text-center">
        <MailCheck aria-hidden="true" className="size-12 text-primary" />
        {/* Same message whether or not the email has an account. */}
        <p role="status" className="text-lg">
          {t('auth.resetEmailSent')}
        </p>
        <Link to="/login" className="inline-flex min-h-touch items-center font-semibold text-primary underline">
          {t('auth.backToSignIn')}
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-md">
      <PageHeader title={t('auth.forgotTitle')} description={t('auth.forgotIntro')} />
      <form noValidate onSubmit={(e) => void onSubmit(e)} className="flex flex-col gap-4">
        <ErrorAlert error={forgot.error} />
        <FormField label={t('auth.email')} error={form.formState.errors.email?.message}>
          <Input type="email" autoComplete="email" inputMode="email" dir="ltr" {...form.register('email')} />
        </FormField>
        <Button type="submit" size="lg" disabled={form.formState.isSubmitting}>
          {t('auth.sendResetLink')}
        </Button>
      </form>
    </div>
  );
}
