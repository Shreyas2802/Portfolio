import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiAuthDelete, apiAuthGet, apiAuthPostForm, apiAuthPut } from '../../lib/api'
import type { PagedResult, Photo } from '../photography/types'
import type { AlbumAdmin } from './useAdminAlbums'

export const ADMIN_PAGE_SIZE = 24

export function useAdminPhotos(page: number) {
  return useQuery({
    queryKey: ['admin-photos', page],
    queryFn: () =>
      apiAuthGet<PagedResult<Photo>>(`/photography/photos?page=${page}&pageSize=${ADMIN_PAGE_SIZE}`),
  })
}

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
      queryClient.invalidateQueries({ queryKey: ['hero-photo'] })
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

/**
 * Extracts the dominant colours from a thumbnail via Canvas API,
 * then PUTs them to /photography/{id}/set-hero.
 *
 * Colour extraction: loads the thumbnail into an off-screen canvas,
 * samples a grid of pixels, clusters into N dominant colours by
 * rounding each channel to the nearest 32 (reduces 16M colours to ~512
 * buckets), picks the top-N by frequency. Pure browser API — no deps.
 */
export function useSetHeroPhoto() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (photo: Photo) => {
      const palette = await extractPalette(photo.thumbnailUrl, 3)
      await apiAuthPut(`/photography/${photo.id}/set-hero`, {
        accentPalette: palette.join(','),
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-photos'] })
      queryClient.invalidateQueries({ queryKey: ['hero-photo'] })
      queryClient.invalidateQueries({ queryKey: ['featured-photos'] })
    },
  })
}

async function extractPalette(imageUrl: string, count: number): Promise<string[]> {
  return new Promise((resolve) => {
    const img = new Image()
    img.crossOrigin = 'anonymous'
    img.onload = () => {
      try {
        const size = 80
        const canvas = document.createElement('canvas')
        canvas.width = size
        canvas.height = size
        const ctx = canvas.getContext('2d')
        if (!ctx) { resolve([]); return }

        ctx.drawImage(img, 0, 0, size, size)
        const { data } = ctx.getImageData(0, 0, size, size)

        // Count pixels per colour bucket (round each channel to nearest 32)
        const buckets = new Map<string, number>()
        for (let i = 0; i < data.length; i += 4) {
          const r = Math.round(data[i]     / 32) * 32
          const g = Math.round(data[i + 1] / 32) * 32
          const b = Math.round(data[i + 2] / 32) * 32
          // Skip near-black and near-white (not useful as accents)
          const brightness = (r + g + b) / 3
          if (brightness < 30 || brightness > 220) continue
          const key = `${r},${g},${b}`
          buckets.set(key, (buckets.get(key) ?? 0) + 1)
        }

        // Sort by frequency, pick top N, convert to hex
        const top = [...buckets.entries()]
          .sort((a, b) => b[1] - a[1])
          .slice(0, count)
          .map(([key]) => {
            const [r, g, b] = key.split(',').map(Number)
            return '#' + [r, g, b].map((v) => v.toString(16).padStart(2, '0')).join('')
          })

        resolve(top)
      } catch {
        resolve([])
      }
    }
    img.onerror = () => resolve([])
    img.src = imageUrl
  })
}
