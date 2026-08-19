import { DataLabel } from '../components/ui/DataLabel'

/** Placeholder route — working contact form arrives in Phase 3. */
export function ContactPage() {
  return (
    <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
      <DataLabel items={['CONTACT']} />
      <h1 className="mt-6 text-4xl font-semibold tracking-tight">Contact</h1>
      <p className="mt-4 max-w-xl text-[var(--color-ink-muted)]">
        A working contact form arrives in Phase 3.
      </p>
    </section>
  )
}
