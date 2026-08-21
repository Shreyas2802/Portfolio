import { useQuery } from '@tanstack/react-query'
import { apiAuthGet } from '../../lib/api'

interface DashboardStats {
  photoCount: number
  albumCount: number
  projectCount: number
  unreadContactMessageCount: number
}

export function useAdminStats() {
  return useQuery({
    queryKey: ['admin-stats'],
    queryFn: () => apiAuthGet<DashboardStats>('/admin/stats'),
  })
}
