import { Construction } from 'lucide-react';
import { useTranslation } from 'react-i18next';

export function ComingSoon() {
  const { t } = useTranslation();
  return (
    <div className="flex flex-col items-center gap-3 rounded-lg border-(length:--border-width) border-dashed bg-card p-8 text-center text-muted-foreground">
      <Construction aria-hidden="true" className="size-10" />
      <p>{t('placeholder.comingSoon')}</p>
    </div>
  );
}
