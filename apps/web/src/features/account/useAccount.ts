import { useMutation, useQueryClient } from '@tanstack/react-query';

import { useApi } from '@/lib/ApiContext';
import { toApiError } from '@/lib/problems';

// BR-38: download everything the app stores about the user as a JSON file.
export function useExportData() {
  const { api } = useApi();
  return useMutation({
    mutationFn: async () => {
      const { data, error, response } = await api.GET('/api/v1/me/export', { cache: 'no-store' });
      if (!data) throw toApiError(response.status, error);
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `dawaya-export-${data.exportedAt.slice(0, 10)}.json`;
      document.body.append(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    },
  });
}

// BR-38: permanent delete. Signs out and clears every cached query afterwards.
export function useDeleteAccount() {
  const { api, session } = useApi();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (password: string) => {
      const { error, response } = await api.POST('/api/v1/me/delete', { body: { password } });
      if (!response.ok) throw toApiError(response.status, error);
    },
    onSuccess: () => {
      session.signOut();
      queryClient.clear();
    },
  });
}
