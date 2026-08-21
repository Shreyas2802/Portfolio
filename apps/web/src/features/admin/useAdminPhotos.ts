import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiAuthDelete, apiAuthGet, apiAuthPostForm } from '../../lib/api'
import type { Photo } from '../photography/types'
import type { AlbumSummary } from '../photography/types'

export function useAdminPhotos() {
  return useQuery({
    queryKey: ['admin-photos'],
    queryFn: () => apiAuthGet<Photo[]>('/photography/photos'),
  })
}

export function useAdminAlbums() {
  return useQuery({
    queryKey: ['admin-albums'],
    queryFn: () => apiAuthGet<AlbumSummary[]>('/photography/albums'),
  })
}

export function useDeletePhoto() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (photoId: number) => apiAuthDelete(`/photography/${photoId}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-photos'] })
      queryClient.invalidateQueries({ queryKey: ['admin-stats'] })
    },
  })
}

export interface UploadPhotoInput {
  file: File
  title: string
  caption: string
  albumId: number
}

export function useUploadPhoto() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: UploadPhotoInput) => {
      const formData = new FormData()
      formData.append('file', input.file)
      formData.append('title', input.title)
      formData.append('caption', input.caption)
      formData.append('albumId', String(input.albumId))
      return apiAuthPostForm<Photo>('/photography/upload', formData)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-photos'] })
      queryClient.invalidateQueries({ queryKey: ['admin-stats'] })
    },
  })
}
