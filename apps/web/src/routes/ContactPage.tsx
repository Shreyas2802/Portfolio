import { DataLabel } from '../components/ui/DataLabel'
import { ContactForm } from '../features/contact/ContactForm'

export function ContactPage() {
  return (
    <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
      <DataLabel items={['CONTACT']} />
      <h1 className="mt-6 text-4xl font-semibold tracking-tight">Get in touch</h1>
      <p className="mt-4 max-w-xl text-[var(--color-ink-muted)]">
        Have a project, an opportunity, or just want to talk photography or code?
        Send a message below, or reach out directly.
      </p>

      <div className="mt-6 flex flex-wrap gap-6">
        <a
          href="mailto:shreyas.gune28@gmail.com"
          className="data-label transition-colors hover:text-[var(--color-brass)]"
        >
          shreyas.gune28@gmail.com
        </a>
        <a
          href="https://linkedin.com/in/shreyas-gune"
          target="_blank"
          rel="noreferrer"
          className="data-label transition-colors hover:text-[var(--color-brass)]"
        >
          LinkedIn
        </a>
      </div>

      <div className="mt-12">
        <ContactForm />
      </div>
    </section>
  )
}
