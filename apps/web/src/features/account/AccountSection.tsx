import { Download, LogOut, Trash2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Link, useNavigate } from 'react-router';

import { ErrorAlert } from '@/components/ErrorAlert';
import { SignInPrompt } from '@/components/SignInPrompt';
import { Button } from '@/components/ui/button';
import { useSession } from '@/features/auth/AuthContext';
import { useLogout } from '@/features/auth/useAuthActions';

import { useExportData } from './useAccount';

export function AccountSection() {
  const { t } = useTranslation();
  const { status, user } = useSession();
  const navigate = useNavigate();
  const logout = useLogout();
  const exportData = useExportData();

  return (
    <section aria-labelledby="account-heading" className="mb-8 flex flex-col gap-4">
      <h2 id="account-heading" className="text-xl font-bold">
        {t('account.title')}
      </h2>
      {status === 'signedOut' ? <SignInPrompt message={t('account.guestMessage')} /> : null}
      {status === 'signedIn' && user ? (
        <>
          <p>
            {t('account.signedInAs')}{' '}
            <span dir="ltr" className="font-semibold">
              {user.email}
            </span>
          </p>
          <ErrorAlert error={exportData.error} />
          <div className="flex flex-col gap-2 sm:flex-row sm:flex-wrap">
            <Button
              variant="outline"
              disabled={exportData.isPending}
              onClick={() => {
                exportData.mutate();
              }}
            >
              <Download aria-hidden="true" />
              {t('account.export')}
            </Button>
            <Button
              variant="outline"
              disabled={logout.isPending}
              onClick={() => {
                void logout.mutateAsync().finally(() => navigate('/'));
              }}
            >
              <LogOut aria-hidden="true" />
              {t('account.signOut')}
            </Button>
            <Button asChild variant="outline" className="text-destructive">
              <Link to="/settings/delete-account">
                <Trash2 aria-hidden="true" />
                {t('account.delete')}
              </Link>
            </Button>
          </div>
        </>
      ) : null}
    </section>
  );
}
