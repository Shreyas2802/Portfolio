import { Link } from 'react-router-dom'
import { DataLabel } from '../components/ui/DataLabel'

/**
 * Homepage skeleton (Phase 1). Real photography and project data
 * comes from the API in Phase 3/4 — this page is structured so those
 * sections can be swapped from static placeholders to live queries
 * without changing the layout.
 */
export function HomePage() {
  return (
    <>
      {/* Hero: the thesis. One line stating what this person is,
          backed by the EXIF-style device rather than a stock
          headline + gradient. */}
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

      {/* Featured work placeholder — real Photo/Album grid arrives in
          Phase 4 once the API serves photos. */}
      <section className="border-t border-[var(--color-line)] px-6 py-24 sm:px-10">
        <div className="mx-auto max-w-[1600px]">
          <DataLabel items={['FEATURED WORK']} />
          <div className="mt-8 grid grid-cols-1 gap-1 sm:grid-cols-2 lg:grid-cols-3">
            {[1, 2, 3, 4, 5, 6].map((i) => (
              <div
                key={i}
                className="aspect-[4/5] w-full border border-dashed border-[var(--color-line)] bg-[var(--color-canvas-raised)]"
                aria-hidden="true"
              />
            ))}
          </div>
          <p className="data-label mt-4">
            Gallery placeholder — real photos arrive in Phase 4 (Photo model, upload, variants).
          </p>
        </div>
      </section>
    </>
  )
}
