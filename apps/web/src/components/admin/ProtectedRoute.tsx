import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../../features/auth/useAuth'

/**
 * Guards everything under /admin. This is a UX convenience only, not
 * a security boundary - the real enforcement is server-side
 * [Authorize(Roles = "Admin")] on every write endpoint. A client-side
 * check can always be bypassed by editing localStorage; it just
 * shouldn't need to be, since the API never trusts it either.
 */
export function ProtectedRoute() {
  const { isAuthenticated } = useAuth()
  if (!isAuthenticated) {
    return <Navigate to="/admin/login" replace />
  }
  return <Outlet />
}
