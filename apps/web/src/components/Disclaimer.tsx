import { Info } from 'lucide-react';
import { useTranslation } from 'react-i18next';

import { cn } from '@/lib/utils';

interface DisclaimerProps {
  className?: string;
}

// BR-23: the fixed disclaimer shown on every medicine page. The text is fixed (no props to change it)
// and comes only from the i18n files, so it cannot drift between pages.
export function Disclaimer({ className }: DisclaimerProps) {
  const { t } = useTranslation();
  return (
    <aside
      role="note"
      aria-labelledby="disclaimer-title"
      className={cn(
        'flex items-start gap-3 rounded-lg border-(length:--border-width) border-notice-border bg-notice p-4 text-notice-foreground',
        className,
      )}
    >
      <Info aria-hidden="true" className="mt-0.5 size-5 shrink-0" />
      <p>
        <strong id="disclaimer-title" className="font-bold">
          {t('disclaimer.title')}:
        </strong>{' '}
        {t('disclaimer.text')}
      </p>
    </aside>
  );
}
