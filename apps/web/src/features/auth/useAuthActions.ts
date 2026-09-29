import { useMutation, useQueryClient } from '@tanstack/react-query';
import type { Schemas } from 'api-client';

import { useApi } from '@/lib/ApiContext';
import { toApiError } from '@/lib/problems';

export function useLogin() {
  const { api, session } = useApi();
  return useMutation({
    mutationFn: async (body: Schemas['LoginRequest']) => {
      const { data, error, response } = await api.POST('/api/v1/auth/login', { body });
      if (!data) throw toApiError(response.status, error);
      session.signIn(data);
      return data;
    },
  });
}

export function useRegister() {
  const { api, session } = useApi();
  return useMutation({
    mutationFn: async (body: Schemas['RegisterRequest']) => {
      const { data, error, response } = await api.POST('/api/v1/auth/register', { body });
      if (!data) throw toApiError(response.status, error);
      session.signIn(data);
      return data;
    },
  });
}

export function useLogout() {
  const { api, session } = useApi();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async () => {
      const refreshToken = session.getRefreshToken();
      // Sign out locally even if the server call fails (e.g. offline).
      if (refreshToken) await api.POST('/api/v1/auth/logout', { body: { refreshToken } }).catch(() => undefined);
    },
    onSettled: () => {
      session.signOut();
      queryClient.clear();
    },
  });
}

export function useForgotPassword() {
  const { api } = useApi();
  return useMutation({
    mutationFn: async (body: Schemas['ForgotPasswordRequest']) => {
      const { error, response } = await api.POST('/api/v1/auth/forgot-password', { body });
      if (!response.ok) throw toApiError(response.status, error);
    },
  });
}

export function useResetPassword() {
  const { api } = useApi();
  return useMutation({
    mutationFn: async (body: Schemas['ResetPasswordRequest']) => {
      const { error, response } = await api.POST('/api/v1/auth/reset-password', { body });
      if (!response.ok) throw toApiError(response.status, error);
    },
  });
}
