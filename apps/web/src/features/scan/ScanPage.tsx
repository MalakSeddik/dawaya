import { useTranslation } from 'react-i18next';

import { ComingSoon } from '@/components/ComingSoon';
import { PageHeader } from '@/components/PageHeader';

export function ScanPage() {
  const { t } = useTranslation();
  return (
    <>
      <PageHeader title={t('nav.scan')} />
      <ComingSoon />
    </>
  );
}
