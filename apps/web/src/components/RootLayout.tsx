import { Outlet } from 'react-router-dom'
import { SiteHeader } from './layout/SiteHeader'
import { SiteFooter } from './layout/SiteFooter'

/**
 * Shared shell for every route. Header/footer live here once so
 * individual pages only ever render their own content — this is the
 * "no business/layout logic duplicated across pages" pattern.
 */
export function RootLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      <SiteHeader />
      <div className="flex-1">
        <Outlet />
      </div>
      <SiteFooter />
    </div>
  )
}
