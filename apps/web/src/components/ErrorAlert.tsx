import { CircleAlert } from 'lucide-react';
import { useTranslation } from 'react-i18next';

import { errorCodeOf } from '@/lib/problems';

interface ErrorAlertProps {
  error: unknown;
}

// Shows a translated message for a failed API call. Announced immediately to screen readers.
export function ErrorAlert({ error }: ErrorAlertProps) {
  const { t } = useTranslation();
  if (!error) return null;
  return (
    <div
      role="alert"
      className="flex items-start gap-2 rounded-md border-(length:--border-width) border-destructive p-3 font-semibold text-destructive"
    >
      <CircleAlert aria-hidden="true" className="mt-0.5 size-5 shrink-0" />
      <p>{t(`errors.${errorCodeOf(error)}`)}</p>
    </div>
  );
}
