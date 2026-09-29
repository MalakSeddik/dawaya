import type { TFunction } from 'i18next';
import { z } from 'zod';

// Mirrors the server rules for quick feedback; the server remains the authority.
export const MIN_PASSWORD_LENGTH = 8;

export function loginSchema(t: TFunction) {
  return z.object({
    email: z.string().trim().min(1, t('validation.required')),
    password: z.string().min(1, t('validation.required')),
  });
}

export function registerSchema(t: TFunction) {
  return z.object({
    displayName: z.string().trim().min(1, t('validation.required')).max(100, t('validation.tooLong')),
    email: z.string().trim().min(1, t('validation.required')).pipe(z.email(t('validation.email'))),
    password: z.string().min(MIN_PASSWORD_LENGTH, t('validation.passwordMin', { count: MIN_PASSWORD_LENGTH })),
  });
}

export function forgotPasswordSchema(t: TFunction) {
  return z.object({
    email: z.string().trim().min(1, t('validation.required')).pipe(z.email(t('validation.email'))),
  });
}

export function resetPasswordSchema(t: TFunction) {
  return z
    .object({
      password: z.string().min(MIN_PASSWORD_LENGTH, t('validation.passwordMin', { count: MIN_PASSWORD_LENGTH })),
      confirm: z.string(),
    })
    .refine((v) => v.password === v.confirm, { message: t('validation.passwordsMatch'), path: ['confirm'] });
}
