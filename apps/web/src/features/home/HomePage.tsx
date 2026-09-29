import { CalendarClock, ChevronLeft, ScanLine, Users, type LucideIcon } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router';

import { Disclaimer } from '@/components/Disclaimer';
import { PageHeader } from '@/components/PageHeader';

interface HomeAction {
  to: string;
  icon: LucideIcon;
  key: 'scan' | 'schedule' | 'profiles';
}

// The main flows start here, one tap from home (BR-35).
const ACTIONS: readonly HomeAction[] = [
  { to: '/scan', icon: ScanLine, key: 'scan' },
  { to: '/schedule', icon: CalendarClock, key: 'schedule' },
  { to: '/profiles', icon: Users, key: 'profiles' },
];

export function HomePage() {
  const { t } = useTranslation();
  return (
    <>
      <PageHeader title={t('home.title')} description={t('home.intro')} />
      <ul className="mb-8 grid gap-3 sm:grid-cols-2">
        {ACTIONS.map(({ to, icon: Icon, key }) => (
          <li key={key}>
            <Link
              to={to}
              className="flex min-h-20 items-center gap-4 rounded-lg border-(length:--border-width) bg-card p-4 hover:bg-accent"
            >
              <span className="flex size-12 shrink-0 items-center justify-center rounded-full bg-secondary text-secondary-foreground">
                <Icon aria-hidden="true" className="size-6" />
              </span>
              <span className="flex flex-1 flex-col">
                <span className="text-lg font-bold">{t(`home.actions.${key}.title`)}</span>
                <span className="text-muted-foreground">{t(`home.actions.${key}.description`)}</span>
              </span>
              {/* Points "forward" in both directions: flipped in LTR */}
              <ChevronLeft aria-hidden="true" className="size-5 shrink-0 ltr:rotate-180" />
            </Link>
          </li>
        ))}
      </ul>
      <Disclaimer />
    </>
  );
}
