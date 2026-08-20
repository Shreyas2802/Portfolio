import { useState } from 'react'
import { DataLabel } from '../components/ui/DataLabel'
import { Lightbox } from '../components/ui/Lightbox'
import { useAlbums, useAlbum } from '../features/photography/usePhotography'

export function WorkPage() {
  const { data: albums, isLoading: albumsLoading } = useAlbums()
  const firstAlbumSlug = albums?.[0]?.slug
  const { data: album, isLoading: albumLoading } = useAlbum(firstAlbumSlug)
  const [activeIndex, setActiveIndex] = useState<number | null>(null)

  const isLoading = albumsLoading || (Boolean(firstAlbumSlug) && albumLoading)

  return (
    <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
      <DataLabel items={['PHOTOGRAPHY']} />
      <h1 className="mt-6 text-4xl font-semibold tracking-tight">Photography</h1>

      {isLoading && (
        <p className="mt-4 text-[var(--color-ink-muted)]">Loading gallery…</p>
      )}

      {!isLoading && !album && (
        <p className="mt-4 text-[var(--color-ink-muted)]">
          No published albums yet — check back soon.
        </p>
      )}

      {album && (
        <>
          <p className="mt-4 max-w-2xl text-[var(--color-ink-muted)]">{album.description}</p>

          <div className="mt-12 columns-1 gap-1 sm:columns-2 lg:columns-3 [&>*]:mb-1">
            {album.photos.map((photo, index) => (
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
        </>
      )}

      {album && activeIndex !== null && (
        <Lightbox
          photos={album.photos}
          activeIndex={activeIndex}
          onClose={() => setActiveIndex(null)}
          onNavigate={setActiveIndex}
        />
      )}
    </section>
  )
}
