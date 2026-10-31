import { Link } from 'react-router-dom'

export function SiteFooter() {
  const year = new Date().getFullYear()

  return (
    <footer className="border-t border-[var(--color-line)] bg-[var(--color-canvas-raised)]">
      <div className="mx-auto max-w-[1600px] px-6 py-12 sm:px-10">
        <div className="flex flex-col gap-10 sm:flex-row sm:items-start sm:justify-between">

          {/* Identity */}
          <div>
            <div className="flex items-center gap-2">
              <span className="h-1.5 w-1.5 rounded-full bg-[var(--color-gold)]" aria-hidden="true" />
              <span className="text-sm font-semibold text-[var(--color-ink)]">Shreyas</span>
            </div>
            <p className="mt-2 max-w-[200px] text-sm leading-relaxed text-[var(--color-ink-muted)]">
              Developer and photographer based in Pune, India.
            </p>
          </div>

          {/* Nav */}
          <nav aria-label="Footer navigation" className="flex flex-col gap-2">
            <p className="text-[0.6875rem] font-medium uppercase tracking-widest text-[var(--color-ink-faint)]">
              Navigate
            </p>
            {[
              { to: '/work',     label: 'Photography' },
              { to: '/projects', label: 'Projects'    },
              { to: '/about',    label: 'About'       },
              { to: '/contact',  label: 'Contact'     },
            ].map((item) => (
              <Link
                key={item.to}
                to={item.to}
                className="text-sm text-[var(--color-ink-muted)] transition-colors hover:text-[var(--color-ink)]"
              >
                {item.label}
              </Link>
            ))}
          </nav>

          {/* Contact */}
          <div className="flex flex-col gap-2">
            <p className="text-[0.6875rem] font-medium uppercase tracking-widest text-[var(--color-ink-faint)]">
              Get in touch
            </p>
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
              LinkedIn
            </a>
            <a
              href="https://github.com/shreyas-gune"
              target="_blank"
              rel="noreferrer"
              className="text-sm text-[var(--color-ink-muted)] transition-colors hover:text-[var(--color-gold)]"
            >
              GitHub
            </a>
          </div>
        </div>

        {/* Bottom bar */}
        <div className="mt-10 flex flex-col gap-1 border-t border-[var(--color-line)] pt-6 sm:flex-row sm:items-center sm:justify-between">
          <p className="text-xs text-[var(--color-ink-faint)]">
            © {year} Shreyas Gune. All rights reserved.
          </p>
          <p className="text-xs text-[var(--color-ink-faint)]">
            Built with ASP.NET Core + React
          </p>
        </div>
      </div>
    </footer>
  )
}
