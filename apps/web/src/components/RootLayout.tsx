import { Outlet } from 'react-router-dom'
import { SiteHeader } from './layout/SiteHeader'
import { SiteFooter } from './layout/SiteFooter'

/**
 * Shared shell for every public route.
 *
 * Accessibility additions:
 * - Skip-to-main-content link: visually hidden until focused,
 *   lets keyboard users bypass the nav on every page load.
 * - <main> landmark: screen reader users navigating by landmark
 *   can jump directly to content.
 */
export function RootLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      {/* Skip link — only visible on focus, always first in tab order */}
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:left-4 focus:top-4 focus:z-50 focus:rounded-md focus:bg-[var(--color-canvas-raised)] focus:px-4 focus:py-2 focus:text-sm focus:font-medium focus:text-[var(--color-ink)] focus:shadow-lg focus:ring-2 focus:ring-[var(--color-gold)] focus:outline-none"
      >
        Skip to main content
      </a>

      <SiteHeader />

      <main id="main-content" className="flex-1" tabIndex={-1}>
        <Outlet />
      </main>

      <SiteFooter />
    </div>
  )
}
