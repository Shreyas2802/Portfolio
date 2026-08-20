import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../../lib/api'
import type { AlbumDetail, AlbumSummary, Photo } from './types'

export function useAlbums() {
  return useQuery({
    queryKey: ['albums'],
    queryFn: () => apiGet<AlbumSummary[]>('/photography/albums'),
  })
}

export function useAlbum(slug: string | undefined) {
  return useQuery({
    queryKey: ['album', slug],
    queryFn: () => apiGet<AlbumDetail>(`/photography/albums/${slug}`),
    enabled: Boolean(slug),
  })
}

export function useFeaturedPhotos(count: number) {
  return useQuery({
    queryKey: ['featured-photos', count],
    queryFn: () => apiGet<Photo[]>(`/photography/featured?count=${count}`),
  })
}
