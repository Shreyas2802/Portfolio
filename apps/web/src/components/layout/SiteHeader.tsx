import { useState } from 'react'
import { NavLink } from 'react-router-dom'

const NAV_ITEMS = [
  { to: '/work',     label: 'Photography' },
  { to: '/projects', label: 'Projects'    },
  { to: '/about',    label: 'About'       },
  { to: '/contact',  label: 'Contact'     },
] as const

/**
 * Hover: a thin gold underline slides in from left on mouseenter,
 * slides out on mouseleave. Active route keeps the underline
 * permanently. No pill backgrounds, no colour flips — just the line.
 * This is the "one distinctive moment, everything else quiet" principle.
 */
export function SiteHeader() {
  const [mobileOpen, setMobileOpen] = useState(false)

  return (
    <header className="sticky top-0 z-40 border-b border-[var(--color-line)] bg-[var(--color-canvas)]/88 backdrop-blur-md">
      <div className="mx-auto flex h-16 max-w-[1600px] items-center justify-between px-6 sm:px-10">

        {/* Logo */}
        <NavLink
          to="/"
          onClick={() => setMobileOpen(false)}
          className="group flex items-center gap-2 text-[0.9375rem] font-semibold tracking-tight text-[var(--color-ink)] transition-colors hover:text-[var(--color-gold)]"
        >
          <span
            className="h-1.5 w-1.5 rounded-full bg-[var(--color-gold)] transition-transform duration-300 group-hover:scale-150"
            aria-hidden="true"
          />
          Shreyas
        </NavLink>

        {/* Desktop nav */}
        <nav aria-label="Primary" className="hidden items-center gap-7 sm:flex">
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `nav-link relative py-1 text-sm font-medium transition-colors ${
                  isActive
                    ? 'nav-link--active text-[var(--color-ink)]'
                    : 'text-[var(--color-ink-muted)] hover:text-[var(--color-ink)]'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        {/* Mobile toggle */}
        <button
          type="button"
          aria-label={mobileOpen ? 'Close menu' : 'Open menu'}
          aria-expanded={mobileOpen}
          aria-controls="mobile-nav"
          onClick={() => setMobileOpen((o) => !o)}
          className="flex h-10 w-10 items-center justify-center text-[var(--color-ink-muted)] transition-colors hover:text-[var(--color-ink)] sm:hidden"
        >
          {mobileOpen
            ? <span className="text-base leading-none">✕</span>
            : <span className="flex flex-col gap-1.5">
                <span className="block h-px w-5 bg-current" />
                <span className="block h-px w-5 bg-current" />
                <span className="block h-px w-3.5 bg-current" />
              </span>
          }
        </button>
      </div>

      {/* Mobile panel */}
      {mobileOpen && (
        <nav
          id="mobile-nav"
          aria-label="Primary"
          className="flex flex-col border-t border-[var(--color-line)] bg-[var(--color-canvas-raised)] px-6 py-3 sm:hidden"
        >
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              onClick={() => setMobileOpen(false)}
              className={({ isActive }) =>
                `border-b border-[var(--color-line)] py-3.5 text-sm font-medium last:border-none transition-colors ${
                  isActive
                    ? 'text-[var(--color-gold)]'
                    : 'text-[var(--color-ink-muted)] hover:text-[var(--color-ink)]'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      )}
    </header>
  )
}
