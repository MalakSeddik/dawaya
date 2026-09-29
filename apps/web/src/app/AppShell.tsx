import { Settings } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Link, NavLink, Outlet } from 'react-router';

import { Button } from '@/components/ui/button';
import { AccessibilityToolbar } from '@/features/preferences/AccessibilityToolbar';
import { cn } from '@/lib/utils';

import { NAV_ITEMS } from './navigation';

// Mobile-first shell: bottom tab bar on phones, sidebar from the md breakpoint up.
export function AppShell() {
  const { t } = useTranslation();

  return (
    <div className="min-h-dvh md:grid md:grid-cols-[16rem_1fr]">
      <a
        href="#main"
        className="sr-only z-50 rounded-md bg-primary px-4 py-2 text-primary-foreground focus:not-sr-only focus:fixed focus:start-2 focus:top-2"
      >
        {t('nav.skipToContent')}
      </a>

      <aside className="hidden border-e-(length:--border-width) bg-card md:block">
        <div className="sticky top-0 flex flex-col gap-6 p-4">
          <Link to="/" className="flex min-h-touch items-center gap-3 rounded-md px-2 text-xl font-bold">
            <img src="/logo.svg" alt="" className="size-9" />
            {t('app.name')}
          </Link>
          <nav aria-label={t('nav.primary')}>
            <ul className="flex flex-col gap-1">
              {NAV_ITEMS.map(({ to, labelKey, icon: Icon }) => (
                <li key={to}>
                  <NavLink
                    to={to}
                    end={to === '/'}
                    className={({ isActive }) =>
                      cn(
                        'flex min-h-touch items-center gap-3 rounded-md px-3 text-base font-medium hover:bg-accent',
                        isActive && 'bg-accent font-bold text-accent-foreground',
                      )
                    }
                  >
                    <Icon aria-hidden="true" className="size-5" />
                    {t(labelKey)}
                  </NavLink>
                </li>
              ))}
            </ul>
          </nav>
        </div>
      </aside>

      <div className="flex min-h-dvh flex-col">
        <header className="sticky top-0 z-10 flex items-center justify-between gap-2 border-b-(length:--border-width) bg-card/95 px-4 py-1 backdrop-blur pt-[max(0.25rem,env(safe-area-inset-top))]">
          {/* Logo only on narrow phones, so the toolbar still fits in large-text mode */}
          <Link to="/" className="flex min-h-touch min-w-touch items-center gap-2 text-lg font-bold md:invisible">
            <img src="/logo.svg" alt="" className="size-8" />
            <span className="sr-only min-[26rem]:not-sr-only">{t('app.name')}</span>
          </Link>
          <div className="flex items-center gap-1">
            <AccessibilityToolbar />
            <Button asChild variant="ghost" size="touch" className="md:hidden">
              <Link to="/settings" aria-label={t('nav.settings')}>
                <Settings aria-hidden="true" />
              </Link>
            </Button>
          </div>
        </header>

        <main id="main" tabIndex={-1} className="mx-auto w-full max-w-3xl flex-1 px-4 pt-4 pb-28 md:pb-8">
          <Outlet />
        </main>
      </div>

      <nav
        aria-label={t('nav.primary')}
        className="fixed inset-x-0 bottom-0 z-10 border-t-(length:--border-width) bg-card pb-[env(safe-area-inset-bottom)] md:hidden"
      >
        <ul className="grid grid-cols-4">
          {NAV_ITEMS.filter((item) => item.inBottomBar).map(({ to, labelKey, icon: Icon }) => (
            <li key={to}>
              <NavLink
                to={to}
                end={to === '/'}
                className={({ isActive }) =>
                  cn(
                    'flex min-h-16 flex-col items-center justify-center gap-1 px-1 text-sm font-medium text-muted-foreground',
                    isActive && 'font-bold text-primary',
                  )
                }
              >
                <Icon aria-hidden="true" className="size-6" />
                {t(labelKey)}
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>
    </div>
  );
}
