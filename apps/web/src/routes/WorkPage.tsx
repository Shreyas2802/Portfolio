import { useState } from 'react'
import { useAlbums, useAlbum, useAlbumPhotos } from '../features/photography/usePhotography'
import { Lightbox } from '../components/ui/Lightbox'

export function WorkPage() {
  const { data: albums, isLoading: albumsLoading } = useAlbums()
  const [selectedSlug, setSelectedSlug] = useState<string | null>(null)
  const activeSlug = selectedSlug ?? albums?.[0]?.slug
  const { data: album, isLoading: albumLoading } = useAlbum(activeSlug)
  const { photos, isLoading: photosLoading, isLoadingMore, hasMore, loadMore } = useAlbumPhotos(activeSlug)
  const [activeIndex, setActiveIndex] = useState<number | null>(null)

  const isLoading = albumsLoading || (Boolean(activeSlug) && (albumLoading || photosLoading))

  return (
    <section aria-label="Photography gallery" className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
      <h1 className="text-4xl font-semibold tracking-tight text-[var(--color-ink)]">
        Photography
      </h1>

      {/* Album tabs */}
      {albums && albums.length > 1 && (
        <div className="mt-8 flex flex-wrap gap-2" role="tablist" aria-label="Albums">
          {albums.map((a) => (
            <button
              key={a.slug}
              type="button"
              role="tab"
              aria-selected={a.slug === activeSlug}
              onClick={() => { setSelectedSlug(a.slug); setActiveIndex(null) }}
              className={`cursor-pointer rounded-full border px-4 py-1.5 text-sm transition-colors ${
                a.slug === activeSlug
                  ? 'border-[var(--color-gold)] text-[var(--color-gold)]'
                  : 'border-[var(--color-line)] text-[var(--color-ink-muted)] hover:border-[var(--color-line-strong)] hover:text-[var(--color-ink)]'
              }`}
            >
              {a.title}
              <span className="ml-1.5 font-mono text-[0.625rem] opacity-60">{a.photoCount}</span>
            </button>
          ))}
        </div>
      )}

      {album?.description && (
        <p className="mt-4 max-w-2xl text-[var(--color-ink-muted)]">{album.description}</p>
      )}

      {isLoading && (
        <div className="mt-12 columns-1 gap-1 sm:columns-2 lg:columns-3 [&>*]:mb-1">
          {[1,2,3,4,5,6].map((i) => (
            <div key={i} className="aspect-[4/3] w-full animate-pulse rounded-sm bg-[var(--color-canvas-raised)]" />
          ))}
        </div>
      )}

      {!isLoading && !album && (
        <p className="mt-10 text-[var(--color-ink-muted)]">No published albums yet.</p>
      )}

      {album && photos.length > 0 && (
        <div className="mt-10 columns-1 gap-1 sm:columns-2 lg:columns-3 [&>*]:mb-1">
          {photos.map((photo, index) => (
            <button
              key={photo.id}
              type="button"
              onClick={() => setActiveIndex(index)}
              className="photo-card group block w-full cursor-pointer overflow-hidden rounded-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-gold)]"
              aria-label={`Open ${photo.title}`}
            >
              <img
                src={photo.thumbnailUrl}
                srcSet={`${photo.thumbnailUrl} 480w, ${photo.mediumUrl} 1200w`}
                sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
                alt={photo.caption || photo.title}
                loading="lazy"
                decoding="async"
                width={photo.width}
                height={photo.height}
                className="w-full transition-transform duration-500 group-hover:scale-[1.02]"
                style={{ aspectRatio: `${photo.width} / ${photo.height}` }}
              />
            </button>
          ))}
        </div>
      )}

      {hasMore && (
        <div className="mt-10 flex justify-center">
          <button
            type="button"
            onClick={loadMore}
            disabled={isLoadingMore}
            className="cursor-pointer rounded-full border border-[var(--color-line)] px-6 py-3 text-sm text-[var(--color-ink-muted)] transition-colors hover:border-[var(--color-gold)] hover:text-[var(--color-gold)] disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isLoadingMore ? 'Loading…' : 'Load more'}
          </button>
        </div>
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
