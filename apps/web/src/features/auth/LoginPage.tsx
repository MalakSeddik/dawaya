import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { Link, useNavigate } from 'react-router';
import type { z } from 'zod';

import { ErrorAlert } from '@/components/ErrorAlert';
import { FormField } from '@/components/FormField';
import { PageHeader } from '@/components/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

import { loginSchema } from './schemas';
import { useLogin } from './useAuthActions';

export function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const login = useLogin();
  const schema = loginSchema(t);
  const form = useForm<z.infer<typeof schema>>({ resolver: zodResolver(schema), defaultValues: { email: '', password: '' } });
  const { errors, isSubmitting } = form.formState;

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await login.mutateAsync(values);
      await navigate('/');
    } catch {
      // Shown by <ErrorAlert> from login.error
    }
  });

  return (
    <div className="mx-auto max-w-md">
      <PageHeader title={t('auth.signInTitle')} description={t('auth.signInIntro')} />
      <form noValidate onSubmit={(e) => void onSubmit(e)} className="flex flex-col gap-4">
        <ErrorAlert error={login.error} />
        <FormField label={t('auth.email')} error={errors.email?.message}>
          <Input type="email" autoComplete="email" inputMode="email" dir="ltr" {...form.register('email')} />
        </FormField>
        <FormField label={t('auth.password')} error={errors.password?.message}>
          <Input type="password" autoComplete="current-password" dir="ltr" {...form.register('password')} />
        </FormField>
        <Button type="submit" size="lg" disabled={isSubmitting}>
          {t('auth.signIn')}
        </Button>
      </form>
      <div className="mt-6 flex flex-col gap-3 text-center">
        <Link to="/forgot-password" className="inline-flex min-h-touch items-center justify-center font-semibold text-primary underline">
          {t('auth.forgotPassword')}
        </Link>
        <p>
          {t('auth.noAccount')}{' '}
          <Link to="/register" className="inline-flex min-h-touch items-center font-semibold text-primary underline">
            {t('auth.createAccount')}
          </Link>
        </p>
      </div>
    </div>
  );
}
