import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiAuthDelete, apiAuthGet, apiAuthPostForm } from '../../lib/api'
import type { PagedResult, Photo } from '../photography/types'
import type { AlbumAdmin } from './useAdminAlbums'

export const ADMIN_PAGE_SIZE = 24

/**
 * Paged, not a flat list (Phase 8) - matches the paginated
 * /photography/photos endpoint. Uses Previous/Next rather than the
 * public gallery's "Load more": an admin management list benefits
 * from precise page numbers and counts more than continuous
 * scrolling does.
 */
export function useAdminPhotos(page: number) {
  return useQuery({
    queryKey: ['admin-photos', page],
    queryFn: () =>
      apiAuthGet<PagedResult<Photo>>(`/photography/photos?page=${page}&pageSize=${ADMIN_PAGE_SIZE}`),
  })
}

/**
 * Used by the upload form's album picker. Deliberately hits the admin
 * "all albums" endpoint, not the public one - the public endpoint only
 * returns published albums, which meant a newly created (or
 * intentionally unpublished) album never showed up here to upload
 * into.
 */
export function useAdminAlbums() {
  return useQuery({
    queryKey: ['admin-albums'],
    queryFn: () => apiAuthGet<AlbumAdmin[]>('/photography/albums/admin/all'),
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
  tagIds: number[]
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
      input.tagIds.forEach((id) => formData.append('tagIds', String(id)))
      return apiAuthPostForm<Photo>('/photography/upload', formData)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-photos'] })
      queryClient.invalidateQueries({ queryKey: ['admin-stats'] })
    },
  })
}
