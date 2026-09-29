import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { Schemas } from 'api-client';

import { useSession } from '@/features/auth/AuthContext';
import { useApi } from '@/lib/ApiContext';
import { toApiError } from '@/lib/problems';

export type Profile = Schemas['ProfileResponse'];
export type SaveProfile = Schemas['SaveProfileRequest'];
export type AgeGroup = NonNullable<Schemas['AgeGroup']>;

export const AGE_GROUPS: readonly AgeGroup[] = ['Infant', 'Child', 'Adult', 'Elderly'];

const keys = {
  all: ['profiles'] as const,
  one: (id: number) => ['profiles', id] as const,
};

export function useProfiles() {
  const { api } = useApi();
  const { status } = useSession();
  return useQuery({
    queryKey: keys.all,
    enabled: status === 'signedIn',
    queryFn: async ({ signal }) => {
      const { data, error, response } = await api.GET('/api/v1/profiles', { signal });
      if (!data) throw toApiError(response.status, error);
      return data;
    },
  });
}

export function useProfile(id: number) {
  const { api } = useApi();
  const { status } = useSession();
  return useQuery({
    queryKey: keys.one(id),
    enabled: status === 'signedIn' && Number.isInteger(id),
    queryFn: async ({ signal }) => {
      const { data, error, response } = await api.GET('/api/v1/profiles/{id}', { params: { path: { id } }, signal });
      if (!data) throw toApiError(response.status, error);
      return data;
    },
  });
}

function useProfileMutation<TVars>(run: (vars: TVars) => Promise<Profile>) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: run,
    onSuccess: async (profile) => {
      queryClient.setQueryData(keys.one(profile.id), profile);
      await queryClient.invalidateQueries({ queryKey: keys.all, exact: true });
    },
  });
}

export function useCreateProfile() {
  const { api } = useApi();
  return useProfileMutation(async (body: SaveProfile) => {
    const { data, error, response } = await api.POST('/api/v1/profiles', { body });
    if (!data) throw toApiError(response.status, error);
    return data;
  });
}

export function useUpdateProfile(id: number) {
  const { api } = useApi();
  return useProfileMutation(async (body: SaveProfile) => {
    const { data, error, response } = await api.PUT('/api/v1/profiles/{id}', { params: { path: { id } }, body });
    if (!data) throw toApiError(response.status, error);
    return data;
  });
}

export function useDeleteProfile(id: number) {
  const { api } = useApi();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async () => {
      const { error, response } = await api.DELETE('/api/v1/profiles/{id}', { params: { path: { id } } });
      if (!response.ok) throw toApiError(response.status, error);
    },
    onSuccess: async () => {
      queryClient.removeQueries({ queryKey: keys.one(id) });
      await queryClient.invalidateQueries({ queryKey: keys.all, exact: true });
    },
  });
}

export function useAddAllergy(profileId: number) {
  const { api } = useApi();
  return useProfileMutation(async (body: Schemas['AddAllergyRequest']) => {
    const { data, error, response } = await api.POST('/api/v1/profiles/{id}/allergies', {
      params: { path: { id: profileId } },
      body,
    });
    if (!data) throw toApiError(response.status, error);
    return data;
  });
}

export function useRemoveAllergy(profileId: number) {
  const { api } = useApi();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (allergyId: number) => {
      const { error, response } = await api.DELETE('/api/v1/profiles/{id}/allergies/{allergyId}', {
        params: { path: { id: profileId, allergyId } },
      });
      if (!response.ok) throw toApiError(response.status, error);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: keys.one(profileId) });
      await queryClient.invalidateQueries({ queryKey: keys.all, exact: true });
    },
  });
}

export function useIngredientSearch(term: string) {
  const { api } = useApi();
  const trimmed = term.trim();
  return useQuery({
    queryKey: ['ingredients', trimmed],
    enabled: trimmed.length >= 2,
    staleTime: 5 * 60_000,
    queryFn: async ({ signal }) => {
      const { data, error, response } = await api.GET('/api/v1/ingredients', {
        params: { query: { search: trimmed } },
        signal,
      });
      if (!data) throw toApiError(response.status, error);
      return data;
    },
  });
}
