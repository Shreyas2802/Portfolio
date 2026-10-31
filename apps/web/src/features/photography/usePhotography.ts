import { useQueries, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { apiGet } from '../../lib/api'
import type { AlbumDetail, AlbumSummary, PagedResult, Photo } from './types'

const PAGE_SIZE = 24

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

/**
 * Fetches the pinned hero photo (or first published photo as fallback).
 * Returns null when there are no photos yet.
 * Used by HomePage to set the hero background and extract accent palette.
 */
export function useHeroPhoto() {
  return useQuery({
    queryKey: ['hero-photo'],
    queryFn: async () => {
      const res = await fetch('/api/photography/hero')
      if (res.status === 204) return null   // no photos yet
      if (!res.ok) throw new Error('Failed to fetch hero photo')
      return res.json() as Promise<Photo>
    },
    staleTime: 60_000,
  })
}

export function useAlbumPhotos(slug: string | undefined) {
  const [page, setPage] = useState(1)
  const [lastSlug, setLastSlug] = useState(slug)

  if (slug !== lastSlug) {
    setLastSlug(slug)
    setPage(1)
  }

  const pageNumbers = Array.from({ length: page }, (_, i) => i + 1)
  const queries = useQueries({
    queries: pageNumbers.map((p) => ({
      queryKey: ['album-photos', slug, p],
      queryFn: () =>
        apiGet<PagedResult<Photo>>(`/photography/albums/${slug}/photos?page=${p}&pageSize=${PAGE_SIZE}`),
      enabled: Boolean(slug),
    })),
  })

  const photos = queries.flatMap((q) => q.data?.items ?? [])
  const lastQuery = queries[queries.length - 1]
  const totalPages = lastQuery?.data?.totalPages ?? 0

  return {
    photos,
    isLoading: queries[0]?.isLoading ?? false,
    isLoadingMore: page > 1 && (lastQuery?.isLoading ?? false),
    hasMore: page < totalPages,
    loadMore: () => setPage((p) => p + 1),
  }
}
