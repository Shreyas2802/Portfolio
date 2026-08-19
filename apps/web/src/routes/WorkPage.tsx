import { DataLabel } from '../components/ui/DataLabel'

/** Placeholder route — full gallery (albums, categories, lightbox) arrives in Phase 4. */
export function WorkPage() {
  return (
    <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
      <DataLabel items={['PHOTOGRAPHY']} />
      <h1 className="mt-6 text-4xl font-semibold tracking-tight">Photography</h1>
      <p className="mt-4 max-w-xl text-[var(--color-ink-muted)]">
        The full gallery — albums, categories, and the lightbox — arrives in Phase 4.
      </p>
    </section>
  )
}
