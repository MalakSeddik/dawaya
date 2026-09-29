import type { RouteObject } from 'react-router';

import { HomePage } from '@/features/home/HomePage';
import { ProfilesPage } from '@/features/profiles/ProfilesPage';
import { ScanPage } from '@/features/scan/ScanPage';
import { SchedulePage } from '@/features/schedule/SchedulePage';
import { SettingsPage } from '@/features/settings/SettingsPage';

import { AppShell } from './AppShell';

export const routes: RouteObject[] = [
  {
    element: <AppShell />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'scan', element: <ScanPage /> },
      { path: 'schedule', element: <SchedulePage /> },
      { path: 'profiles', element: <ProfilesPage /> },
      { path: 'settings', element: <SettingsPage /> },
      { path: '*', element: <HomePage /> },
    ],
  },
];
