import { Link } from 'react-router-dom'
import { DataLabel } from '../components/ui/DataLabel'
import { useFeaturedPhotos } from '../features/photography/usePhotography'

/**
 * Homepage. Hero is static copy; the featured-work section pulls real
 * photos from the API (Phase 4) instead of the placeholder grid used
 * in Phase 1.
 */
export function HomePage() {
  const { data: photos, isLoading } = useFeaturedPhotos(6)

  return (
    <>
      <section className="mx-auto flex min-h-[calc(100vh-4rem)] max-w-[1600px] flex-col justify-center px-6 py-24 sm:px-10">
        <DataLabel items={['PHOTOGRAPHER', 'SOFTWARE DEVELOPER', 'PUNE, INDIA']} />
        <h1 className="mt-6 max-w-4xl text-5xl font-semibold leading-[1.05] tracking-tight sm:text-7xl">
          I build software.
          <br />
          I frame moments.
        </h1>
        <p className="mt-6 max-w-xl text-lg text-[var(--color-ink-muted)]">
          A working developer and a photographer, in one place. Browse the
          photography, or see how this site — and the systems behind it —
          were built.
        </p>
        <div className="mt-10 flex flex-wrap gap-4">
          <Link
            to="/work"
            className="rounded-full bg-[var(--color-ink)] px-6 py-3 text-sm font-medium text-[var(--color-canvas)] transition-opacity hover:opacity-90"
          >
            View Photography
          </Link>
          <Link
            to="/projects"
            className="data-label rounded-full border border-[var(--color-line)] px-6 py-3 transition-colors hover:border-[var(--color-brass)] hover:text-[var(--color-brass)]"
          >
            View Projects
          </Link>
        </div>
      </section>

      <section className="border-t border-[var(--color-line)] px-6 py-24 sm:px-10">
        <div className="mx-auto max-w-[1600px]">
          <DataLabel items={['FEATURED WORK']} />

          {isLoading && (
            <div className="mt-8 grid grid-cols-1 gap-1 sm:grid-cols-2 lg:grid-cols-3">
              {[1, 2, 3, 4, 5, 6].map((i) => (
                <div
                  key={i}
                  className="aspect-[4/5] w-full animate-pulse bg-[var(--color-canvas-raised)]"
                />
              ))}
            </div>
          )}

          {photos && photos.length > 0 && (
            <Link to="/work" className="mt-8 grid grid-cols-1 gap-1 sm:grid-cols-2 lg:grid-cols-3">
              {photos.map((photo) => (
                <img
                  key={photo.id}
                  src={photo.thumbnailUrl}
                  srcSet={`${photo.thumbnailUrl} 480w, ${photo.mediumUrl} 1200w`}
                  sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
                  alt={photo.caption || photo.title}
                  loading="lazy"
                  className="aspect-[4/5] w-full object-cover transition-opacity hover:opacity-90"
                />
              ))}
            </Link>
          )}

          {photos && photos.length === 0 && !isLoading && (
            <p className="data-label mt-4">No published photos yet.</p>
          )}
        </div>
      </section>
    </>
  )
}
