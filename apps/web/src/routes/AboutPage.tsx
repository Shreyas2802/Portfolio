import { DataLabel } from '../components/ui/DataLabel'

/** Placeholder route — bio, skills, resume arrive in Phase 3. */
export function AboutPage() {
  return (
    <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
      <DataLabel items={['ABOUT']} />
      <h1 className="mt-6 text-4xl font-semibold tracking-tight">About</h1>
      <p className="mt-4 max-w-xl text-[var(--color-ink-muted)]">
        Bio, skills, and resume arrive in Phase 3.
      </p>
    </section>
  )
}
