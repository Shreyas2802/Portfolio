import { ContactForm } from '../features/contact/ContactForm'

export function ContactPage() {
  return (
    <section aria-label="Contact" className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
      <h1 className="text-4xl font-semibold tracking-tight text-[var(--color-ink)]">
        Get in touch
      </h1>
      <p className="mt-4 max-w-xl text-[var(--color-ink-muted)]">
        Have a project, an opportunity, or just want to talk photography or code?
        Send a message below, or reach out directly.
      </p>

      <div className="mt-6 flex flex-wrap gap-6">
        <a
          href="mailto:shreyas.gune28@gmail.com"
          className="text-sm text-[var(--color-ink-muted)] transition-colors hover:text-[var(--color-gold)]"
        >
          shreyas.gune28@gmail.com
        </a>
        <a
          href="https://linkedin.com/in/shreyas-gune"
          target="_blank"
          rel="noreferrer"
          className="text-sm text-[var(--color-ink-muted)] transition-colors hover:text-[var(--color-gold)]"
        >
          LinkedIn ↗
        </a>
      </div>

      <div className="mt-12">
        <ContactForm />
      </div>
    </section>
  )
}
