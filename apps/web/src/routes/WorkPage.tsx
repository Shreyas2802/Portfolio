import { useState } from 'react'
import { DataLabel } from '../components/ui/DataLabel'
import { Lightbox } from '../components/ui/Lightbox'
import { useAlbums, useAlbum, useAlbumPhotos } from '../features/photography/usePhotography'

/**
 * Gallery page. Shows every published album as a set of tabs, and
 * loads that album's photos a page at a time (Phase 8) rather than
 * all at once - the earlier version fetched every photo in the album
 * in a single request, which doesn't scale once an album holds
 * hundreds of photos.
 */
export function WorkPage() {
  const { data: albums, isLoading: albumsLoading } = useAlbums()
  const [selectedSlug, setSelectedSlug] = useState<string | null>(null)
  const activeSlug = selectedSlug ?? albums?.[0]?.slug
  const { data: album, isLoading: albumLoading } = useAlbum(activeSlug)
  const { photos, isLoading: photosLoading, isLoadingMore, hasMore, loadMore } = useAlbumPhotos(activeSlug)
  const [activeIndex, setActiveIndex] = useState<number | null>(null)

  const isLoading = albumsLoading || (Boolean(activeSlug) && (albumLoading || photosLoading))

  return (
    <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
      <DataLabel items={['PHOTOGRAPHY']} />
      <h1 className="mt-6 text-4xl font-semibold tracking-tight">Photography</h1>

      {albums && albums.length > 1 && (
        <div className="mt-8 flex flex-wrap gap-2">
          {albums.map((a) => (
            <button
              key={a.slug}
              type="button"
              onClick={() => {
                setSelectedSlug(a.slug)
                setActiveIndex(null)
              }}
              className={`data-label rounded-full border px-4 py-2 transition-colors ${
                a.slug === activeSlug
                  ? 'border-[var(--color-brass)] text-[var(--color-brass)]'
                  : 'border-[var(--color-line)] text-[var(--color-ink-muted)] hover:text-[var(--color-ink)]'
              }`}
            >
              {a.title} ({a.photoCount})
            </button>
          ))}
        </div>
      )}

      {isLoading && (
        <p className="mt-8 text-[var(--color-ink-muted)]">Loading gallery…</p>
      )}

      {!isLoading && !album && (
        <p className="mt-8 text-[var(--color-ink-muted)]">
          No published albums yet — check back soon.
        </p>
      )}

      {album && (
        <>
          <p className="mt-4 max-w-2xl text-[var(--color-ink-muted)]">{album.description}</p>

          <div className="mt-12 columns-1 gap-1 sm:columns-2 lg:columns-3 [&>*]:mb-1">
            {photos.map((photo, index) => (
              <button
                key={photo.id}
                type="button"
                onClick={() => setActiveIndex(index)}
                className="group block w-full overflow-hidden rounded-sm border border-transparent transition-colors hover:border-[var(--color-brass)]"
              >
                <img
                  src={photo.mediumUrl}
                  srcSet={`${photo.thumbnailUrl} 480w, ${photo.mediumUrl} 1200w, ${photo.largeUrl} 2400w`}
                  sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
                  alt={photo.caption || photo.title}
                  loading="lazy"
                  className="w-full transition-transform duration-300 group-hover:scale-[1.02]"
                  style={{ aspectRatio: `${photo.width} / ${photo.height}` }}
                />
              </button>
            ))}
          </div>

          {hasMore && (
            <div className="mt-8 flex justify-center">
              <button
                type="button"
                onClick={loadMore}
                disabled={isLoadingMore}
                className="data-label rounded-full border border-[var(--color-line)] px-6 py-3 hover:border-[var(--color-brass)] hover:text-[var(--color-brass)] disabled:opacity-50"
              >
                {isLoadingMore ? 'Loading…' : 'Load more'}
              </button>
            </div>
          )}
        </>
      )}

      {album && activeIndex !== null && (
        <Lightbox
          photos={photos}
          activeIndex={activeIndex}
          onClose={() => setActiveIndex(null)}
          onNavigate={setActiveIndex}
        />
      )}
    </section>
  )
}
