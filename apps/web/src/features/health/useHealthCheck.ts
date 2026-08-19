import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../../lib/api'

interface HealthResponse {
  status: string
  timestampUtc: string
}

/**
 * TanStack Query hook for the API's /api/health endpoint.
 * This is the Phase 0 "can the SPA talk to the API" smoke test —
 * real domain queries (photos, albums, projects) will follow this
 * same pattern (one hook per resource in its own feature folder)
 * once those endpoints exist in later phases.
 */
export function useHealthCheck() {
  return useQuery({
    queryKey: ['health'],
    queryFn: () => apiGet<HealthResponse>('/health'),
    retry: 1,
  })
}
