import { useTranslation } from 'react-i18next';

import { ComingSoon } from '@/components/ComingSoon';
import { PageHeader } from '@/components/PageHeader';

export function ProfilesPage() {
  const { t } = useTranslation();
  return (
    <>
      <PageHeader title={t('nav.profiles')} />
      <ComingSoon />
    </>
  );
}
