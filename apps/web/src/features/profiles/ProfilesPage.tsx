import { CircleCheck, Pencil, Plus, TriangleAlert } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';

import { ErrorAlert } from '@/components/ErrorAlert';
import { PageHeader } from '@/components/PageHeader';
import { SignInPrompt } from '@/components/SignInPrompt';
import { Button } from '@/components/ui/button';
import { useSession } from '@/features/auth/AuthContext';

import { allergyLabel } from './allergyLabel';
import { useActiveProfile } from './useActiveProfile';
import { useProfiles, type Profile } from './useProfiles';

export function ProfilesPage() {
  const { t } = useTranslation();
  const { status } = useSession();

  return (
    <>
      <PageHeader title={t('profiles.title')} description={t('profiles.intro')} />
      {status === 'loading' ? <p role="status">{t('common.loading')}</p> : null}
      {status === 'signedOut' ? <SignInPrompt message={t('profiles.signInToManage')} /> : null}
      {status === 'signedIn' ? <ProfileList /> : null}
    </>
  );
}

function ProfileList() {
  const { t } = useTranslation();
  const { error, isPending } = useProfiles();
  const { activeProfile, profiles, setActiveProfileId } = useActiveProfile();

  if (isPending) return <p role="status">{t('common.loading')}</p>;
  if (error) return <ErrorAlert error={error} />;

  return (
    <div className="flex flex-col gap-4">
      <ul className="flex flex-col gap-3">
        {profiles?.map((profile) => (
          <li key={profile.id}>
            <ProfileCard
              profile={profile}
              isActive={profile.id === activeProfile?.id}
              onActivate={() => {
                setActiveProfileId(profile.id);
              }}
            />
          </li>
        ))}
      </ul>
      <Button asChild size="lg">
        <Link to="/profiles/new">
          <Plus aria-hidden="true" />
          {t('profiles.add')}
        </Link>
      </Button>
    </div>
  );
}

interface ProfileCardProps {
  profile: Profile;
  isActive: boolean;
  onActivate: () => void;
}

function ProfileCard({ profile, isActive, onActivate }: ProfileCardProps) {
  const { t, i18n } = useTranslation();
  const details = [
    profile.relationship,
    profile.ageGroup ? t(`ageGroups.${profile.ageGroup}.label`) : null,
    profile.isPregnant ? t('profiles.pregnant') : null,
    profile.isBreastfeeding ? t('profiles.breastfeeding') : null,
  ].filter(Boolean);

  return (
    <article
      aria-labelledby={`profile-${profile.id}`}
      className="flex flex-col gap-3 rounded-lg border-(length:--border-width) bg-card p-4 aria-[current=true]:border-primary"
      aria-current={isActive}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="flex flex-col">
          <h2 id={`profile-${profile.id}`} className="text-xl font-bold">
            {profile.isSelf ? t('profiles.meLabel', { name: profile.displayName }) : profile.displayName}
          </h2>
          {details.length > 0 ? <p className="text-muted-foreground">{details.join(' · ')}</p> : null}
        </div>
        <Button asChild variant="ghost" size="touch">
          <Link to={`/profiles/${profile.id}`} aria-label={t('profiles.editNamed', { name: profile.displayName })}>
            <Pencil aria-hidden="true" />
          </Link>
        </Button>
      </div>

      {profile.allergies.length > 0 ? (
        <p className="flex items-start gap-2 font-semibold text-destructive">
          <TriangleAlert aria-hidden="true" className="mt-0.5 size-5 shrink-0" />
          <span>
            {t('profiles.allergicTo')}{' '}
            {profile.allergies.map((a) => allergyLabel(a, i18n.language)).join(i18n.language === 'ar' ? '، ' : ', ')}
          </span>
        </p>
      ) : null}

      {isActive ? (
        <p className="flex items-center gap-2 font-semibold text-primary">
          <CircleCheck aria-hidden="true" className="size-5" />
          {t('profiles.isActive')}
        </p>
      ) : (
        <Button variant="outline" onClick={onActivate} className="self-start">
          {t('profiles.makeActive')}
        </Button>
      )}
    </article>
  );
}
