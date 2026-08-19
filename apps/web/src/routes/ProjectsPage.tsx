import { DataLabel } from '../components/ui/DataLabel'

/** Placeholder route — project case studies arrive in Phase 3. */
export function ProjectsPage() {
  return (
    <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
      <DataLabel items={['PROJECTS']} />
      <h1 className="mt-6 text-4xl font-semibold tracking-tight">Projects</h1>
      <p className="mt-4 max-w-xl text-[var(--color-ink-muted)]">
        Case studies for Brahmanda Tatva and this site itself arrive in Phase 3.
      </p>
    </section>
  )
}
