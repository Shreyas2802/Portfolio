import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiAuthDelete, apiAuthGet, apiAuthPostJson, apiAuthPutJson } from '../../lib/api'

export interface AlbumAdmin {
  id: number
  title: string
  slug: string
  description: string
  categoryId: number | null
  categoryName: string | null
  isPublished: boolean
  photoCount: number
}

export interface AlbumFormInput {
  title: string
  description: string
  categoryId: number | null
  isPublished: boolean
}

export function useAdminAlbumsList() {
  return useQuery({
    queryKey: ['admin-albums-list'],
    queryFn: () => apiAuthGet<AlbumAdmin[]>('/photography/albums/admin/all'),
  })
}

function invalidateAlbumQueries(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: ['admin-albums-list'] })
  queryClient.invalidateQueries({ queryKey: ['admin-albums'] })
  queryClient.invalidateQueries({ queryKey: ['albums'] })
  queryClient.invalidateQueries({ queryKey: ['admin-stats'] })
}

export function useCreateAlbum() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: AlbumFormInput) =>
      apiAuthPostJson<AlbumFormInput, AlbumAdmin>('/photography/albums', input),
    onSuccess: () => invalidateAlbumQueries(queryClient),
  })
}

export function useUpdateAlbum() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: AlbumFormInput }) =>
      apiAuthPutJson<AlbumFormInput, AlbumAdmin>(`/photography/albums/${id}`, input),
    onSuccess: () => invalidateAlbumQueries(queryClient),
  })
}

export function useDeleteAlbum() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => apiAuthDelete(`/photography/albums/${id}`),
    onSuccess: () => invalidateAlbumQueries(queryClient),
  })
}
