import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../../features/auth/useAuth'

const NAV_ITEMS = [
  { to: '/admin', label: 'Dashboard', end: true },
  { to: '/admin/photos', label: 'Photos' },
  { to: '/admin/albums', label: 'Albums' },
  { to: '/admin/taxonomy', label: 'Categories & Tags' },
  { to: '/admin/projects', label: 'Projects' },
] as const

export function AdminLayout() {
  const { displayName, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/admin/login')
  }

  return (
    <div className="flex min-h-screen flex-col">
      <header className="border-b border-[var(--color-line)]">
        <div className="mx-auto flex h-16 max-w-[1600px] items-center justify-between px-6 sm:px-10">
          <nav aria-label="Admin" className="flex gap-6">
            {NAV_ITEMS.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={'end' in item ? item.end : false}
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
          <div className="flex items-center gap-4">
            <span className="data-label text-[var(--color-ink-muted)]">{displayName}</span>
            <button
              type="button"
              onClick={handleLogout}
              className="data-label rounded-full border border-[var(--color-line)] px-4 py-2 hover:border-[var(--color-brass)] hover:text-[var(--color-brass)]"
            >
              Log out
            </button>
          </div>
        </div>
      </header>
      <div className="flex-1">
        <Outlet />
      </div>
    </div>
  )
}
