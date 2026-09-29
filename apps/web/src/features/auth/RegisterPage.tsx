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
import { usePreferences } from '@/features/preferences/usePreferences';

import { MIN_PASSWORD_LENGTH, registerSchema } from './schemas';
import { useRegister } from './useAuthActions';

export function RegisterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const register = useRegister();
  const { preferences } = usePreferences();
  const schema = registerSchema(t);
  const form = useForm<z.infer<typeof schema>>({
    resolver: zodResolver(schema),
    defaultValues: { displayName: '', email: '', password: '' },
  });
  const { errors, isSubmitting } = form.formState;

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      // Keep the display settings chosen on this device (BR-33).
      await register.mutateAsync({
        ...values,
        preferredLanguage: preferences.language,
        largeText: preferences.largeText,
        highContrast: preferences.highContrast,
      });
      await navigate('/profiles');
    } catch {
      // Shown by <ErrorAlert>
    }
  });

  return (
    <div className="mx-auto max-w-md">
      <PageHeader title={t('auth.registerTitle')} description={t('auth.registerIntro')} />
      <form noValidate onSubmit={(e) => void onSubmit(e)} className="flex flex-col gap-4">
        <ErrorAlert error={register.error} />
        <FormField label={t('auth.yourName')} hint={t('auth.yourNameHint')} error={errors.displayName?.message}>
          <Input autoComplete="name" {...form.register('displayName')} />
        </FormField>
        <FormField label={t('auth.email')} error={errors.email?.message}>
          <Input type="email" autoComplete="email" inputMode="email" dir="ltr" {...form.register('email')} />
        </FormField>
        <FormField
          label={t('auth.password')}
          hint={t('auth.passwordHint', { count: MIN_PASSWORD_LENGTH })}
          error={errors.password?.message}
        >
          <Input type="password" autoComplete="new-password" dir="ltr" {...form.register('password')} />
        </FormField>
        <Button type="submit" size="lg" disabled={isSubmitting}>
          {t('auth.createAccount')}
        </Button>
      </form>
      <p className="mt-6 text-center">
        {t('auth.haveAccount')}{' '}
        <Link to="/login" className="inline-flex min-h-touch items-center font-semibold text-primary underline">
          {t('auth.signIn')}
        </Link>
      </p>
    </div>
  );
}
