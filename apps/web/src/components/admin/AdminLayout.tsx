import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../../features/auth/useAuth'
import { useAdminStats } from '../../features/admin/useAdminStats'

const NAV_ITEMS = [
  { to: '/admin', label: 'Dashboard', end: true },
  { to: '/admin/photos', label: 'Photos' },
  { to: '/admin/albums', label: 'Albums' },
  { to: '/admin/taxonomy', label: 'Categories & Tags' },
  { to: '/admin/projects', label: 'Projects' },
  { to: '/admin/messages', label: 'Messages' },
] as const

export function AdminLayout() {
  const { displayName, logout } = useAuth()
  const navigate = useNavigate()
  const { data: stats } = useAdminStats()
  const unread = stats?.unreadContactMessageCount ?? 0

  function handleLogout() {
    logout()
    navigate('/admin/login')
  }

  return (
    <div className="flex min-h-screen flex-col bg-[var(--color-canvas)]">
      <header className="sticky top-0 z-40 border-b border-[var(--color-line)] bg-[var(--color-canvas)]/90 backdrop-blur-md">
        <div className="mx-auto flex h-14 max-w-[1600px] items-center justify-between gap-4 px-6 sm:px-10">

          <div className="flex items-center gap-6 overflow-x-auto">
            <NavLink
              to="/"
              className="flex shrink-0 items-center gap-1.5 font-semibold tracking-tight text-[var(--color-ink)] transition-colors hover:text-[var(--color-gold)]"
            >
              <span className="h-1.5 w-1.5 rounded-full bg-[var(--color-gold)]" aria-hidden="true" />
              Shreyas
            </NavLink>

            <span className="h-4 w-px shrink-0 bg-[var(--color-line-strong)]" aria-hidden="true" />

            <nav aria-label="Admin navigation" className="flex items-center gap-1">
              {NAV_ITEMS.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={'end' in item ? item.end : false}
                  className={({ isActive }) =>
                    `relative rounded-md px-3 py-1.5 font-mono text-[0.6875rem] uppercase tracking-[0.08em] transition-colors ${
                      isActive
                        ? 'bg-[var(--color-canvas-hover)] text-[var(--color-gold)]'
                        : 'text-[var(--color-ink-muted)] hover:bg-[var(--color-canvas-hover)] hover:text-[var(--color-ink)]'
                    }`
                  }
                >
                  {item.label}
                  {/* Unread badge on Messages */}
                  {item.to === '/admin/messages' && unread > 0 && (
                    <span className="absolute -right-1 -top-1 flex h-4 w-4 items-center justify-center rounded-full bg-[var(--color-gold)] font-mono text-[0.5rem] font-bold text-[var(--color-canvas)]">
                      {unread > 9 ? '9+' : unread}
                    </span>
                  )}
                </NavLink>
              ))}
            </nav>
          </div>

          <div className="flex shrink-0 items-center gap-3">
            <span className="hidden font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-muted)] sm:block">
              {displayName}
            </span>
            <button
              type="button"
              onClick={handleLogout}
              className="rounded-md border border-[var(--color-line)] px-3 py-1.5 font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-muted)] transition-colors hover:border-[var(--color-gold)] hover:text-[var(--color-gold)]"
            >
              Log out
            </button>
          </div>
        </div>
      </header>

      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  )
}
