import type { RouteObject } from 'react-router';

import { DeleteAccountPage } from '@/features/account/DeleteAccountPage';
import { ForgotPasswordPage } from '@/features/auth/ForgotPasswordPage';
import { LoginPage } from '@/features/auth/LoginPage';
import { RegisterPage } from '@/features/auth/RegisterPage';
import { ResetPasswordPage } from '@/features/auth/ResetPasswordPage';
import { HomePage } from '@/features/home/HomePage';
import { ProfileFormPage } from '@/features/profiles/ProfileFormPage';
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
      { path: 'profiles/new', element: <ProfileFormPage /> },
      { path: 'profiles/:id', element: <ProfileFormPage /> },
      { path: 'settings', element: <SettingsPage /> },
      { path: 'settings/delete-account', element: <DeleteAccountPage /> },
      { path: 'login', element: <LoginPage /> },
      { path: 'register', element: <RegisterPage /> },
      { path: 'forgot-password', element: <ForgotPasswordPage /> },
      { path: 'reset-password', element: <ResetPasswordPage /> },
      { path: '*', element: <HomePage /> },
    ],
  },
];
