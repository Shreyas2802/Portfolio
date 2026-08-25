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
 * "Load more" pagination for an album's photos (Phase 8 - the spec
 * requires bounded, server-enforced pagination on collection
 * endpoints; a masonry gallery reads more naturally as incremental
 * loading than numbered pages).
 *
 * Fetches pages 1..currentPage as separate cached queries (via
 * useQueries) and flattens them into one list on each render, rather
 * than accumulating into local state from an effect - React's own
 * guidance is to avoid setState-in-effect where a value can just be
 * derived during render instead (see useDeletePhoto/etc. for the
 * effect-free "adjust state during render" pattern used for the
 * slug-change reset below).
 */
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
