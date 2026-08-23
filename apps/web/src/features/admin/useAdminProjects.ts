import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiAuthDelete, apiAuthGet, apiAuthPostJson, apiAuthPutJson } from '../../lib/api'

export interface ProjectAdmin {
  id: number
  title: string
  slug: string
  tagline: string
  role: string
  status: string
  overview: string
  contributions: string[]
  technologies: string[]
  githubUrl: string | null
  liveUrl: string | null
  coverImageUrl: string | null
  isFeatured: boolean
  isPublished: boolean
}

export interface ProjectFormInput {
  title: string
  tagline: string
  role: string
  status: string
  overview: string
  contributionsText: string
  technologies: string
  githubUrl: string
  liveUrl: string
  isFeatured: boolean
  isPublished: boolean
}

export function useAdminProjects() {
  return useQuery({
    queryKey: ['admin-projects'],
    queryFn: () => apiAuthGet<ProjectAdmin[]>('/projects/admin/all'),
  })
}

function invalidate(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: ['admin-projects'] })
  queryClient.invalidateQueries({ queryKey: ['projects'] })
  queryClient.invalidateQueries({ queryKey: ['admin-stats'] })
}

export function useCreateProject() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: ProjectFormInput) =>
      apiAuthPostJson<ProjectFormInput, ProjectAdmin>('/projects', input),
    onSuccess: () => invalidate(queryClient),
  })
}

export function useUpdateProject() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: ProjectFormInput }) =>
      apiAuthPutJson<ProjectFormInput, ProjectAdmin>(`/projects/${id}`, input),
    onSuccess: () => invalidate(queryClient),
  })
}

export function useDeleteProject() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => apiAuthDelete(`/projects/${id}`),
    onSuccess: () => invalidate(queryClient),
  })
}
