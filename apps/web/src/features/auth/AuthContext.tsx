import { useCallback, useState, type ReactNode } from 'react'
import { loadStoredAuth, saveStoredAuth, clearStoredAuth, type AuthState } from '../../lib/authStorage'
import { AuthContext } from './authContextInstance'

/**
 * Minimal JWT auth context: holds the token in memory + localStorage
 * (so a page refresh doesn't log the admin out), and exposes login/
 * logout. There's exactly one admin account for this site (see
 * IdentitySeeder on the backend) so this deliberately doesn't handle
 * multi-user concerns like refresh tokens or session lists.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<AuthState>(loadStoredAuth)

  const login = useCallback((token: string, displayName: string) => {
    const next = { token, displayName }
    setAuth(next)
    saveStoredAuth(next)
  }, [])

  const logout = useCallback(() => {
    setAuth({ token: null, displayName: null })
    clearStoredAuth()
  }, [])

  return (
    <AuthContext.Provider
      value={{ ...auth, isAuthenticated: Boolean(auth.token), login, logout }}
    >
      {children}
    </AuthContext.Provider>
  )
}
