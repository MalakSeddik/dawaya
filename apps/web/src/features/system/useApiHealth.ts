import { useQuery } from '@tanstack/react-query';

import { api } from '@/lib/api';

export function useApiHealth() {
  return useQuery({
    queryKey: ['system', 'health'],
    queryFn: async ({ signal }) => {
      const { data, error, response } = await api.GET('/api/v1/health', { signal });
      if (data) return data;
      // A 503 from the API still carries a HealthResponse describing what is down.
      if (response.status === 503 && typeof error === 'object') return error;
      // Anything else (proxy error, API not running) is "no connection".
      throw new Error(`Health check failed with HTTP ${response.status}`);
    },
    retry: false,
  });
}
