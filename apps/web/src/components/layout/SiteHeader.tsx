import { useState } from 'react'
import { NavLink } from 'react-router-dom'

const NAV_ITEMS = [
  { to: '/work', label: 'Photography' },
  { to: '/projects', label: 'Projects' },
  { to: '/about', label: 'About' },
  { to: '/contact', label: 'Contact' },
] as const

/**
 * Site-wide header. Kept visually quiet — a hairline border and
 * nothing else — so it never competes with photography on the pages
 * beneath it. Nav labels reuse the same mono "data-label" treatment
 * as the EXIF/technical labels elsewhere, tying the chrome to the
 * site's signature device instead of looking like a separate system.
 */
export function SiteHeader() {
  const [mobileOpen, setMobileOpen] = useState(false)

  return (
    <header className="sticky top-0 z-40 border-b border-[var(--color-line)] bg-[var(--color-canvas)]/90 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-[1600px] items-center justify-between px-6 sm:px-10">
        <NavLink
          to="/"
          className="font-semibold tracking-tight text-[var(--color-ink)] transition-colors hover:text-[var(--color-brass)]"
          onClick={() => setMobileOpen(false)}
        >
          Shreyas
        </NavLink>

        <nav aria-label="Primary" className="hidden gap-8 sm:flex">
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `data-label transition-colors ${
                  isActive ? 'text-[var(--color-brass)]' : 'text-[var(--color-ink-muted)] hover:text-[var(--color-ink)]'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <button
          type="button"
          className="data-label flex h-11 w-11 items-center justify-center sm:hidden"
          aria-label={mobileOpen ? 'Close menu' : 'Open menu'}
          aria-expanded={mobileOpen}
          aria-controls="mobile-nav"
          onClick={() => setMobileOpen((open) => !open)}
        >
          {mobileOpen ? 'Close' : 'Menu'}
        </button>
      </div>

      {mobileOpen && (
        <nav
          id="mobile-nav"
          aria-label="Primary"
          className="flex flex-col border-t border-[var(--color-line)] px-6 py-4 sm:hidden"
        >
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              onClick={() => setMobileOpen(false)}
              className={({ isActive }) =>
                `data-label border-b border-[var(--color-line)] py-4 last:border-none ${
                  isActive ? 'text-[var(--color-brass)]' : 'text-[var(--color-ink-muted)]'
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
