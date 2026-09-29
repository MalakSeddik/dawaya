import { CalendarClock, House, ScanLine, Settings, Users, type LucideIcon } from 'lucide-react';

export interface NavItem {
  to: string;
  labelKey: 'nav.home' | 'nav.scan' | 'nav.schedule' | 'nav.profiles' | 'nav.settings';
  icon: LucideIcon;
  // Phones show at most four tabs; Settings is reached from the header there.
  inBottomBar: boolean;
}

export const NAV_ITEMS: readonly NavItem[] = [
  { to: '/', labelKey: 'nav.home', icon: House, inBottomBar: true },
  { to: '/scan', labelKey: 'nav.scan', icon: ScanLine, inBottomBar: true },
  { to: '/schedule', labelKey: 'nav.schedule', icon: CalendarClock, inBottomBar: true },
  { to: '/profiles', labelKey: 'nav.profiles', icon: Users, inBottomBar: true },
  { to: '/settings', labelKey: 'nav.settings', icon: Settings, inBottomBar: false },
];
