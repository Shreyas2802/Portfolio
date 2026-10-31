import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../../lib/api'
import type { Project } from './types'

export function useProjects() {
  return useQuery({
    queryKey: ['projects'],
    queryFn: () => apiGet<Project[]>('/projects'),
  })
}

export function useProject(slug: string | undefined) {
  return useQuery({
    queryKey: ['project', slug],
    queryFn: () => apiGet<Project>(`/projects/${slug}`),
    enabled: Boolean(slug),
  })
}
