import { createContext } from 'react'
import type { AuthState } from '../../lib/authStorage'

export interface AuthContextValue extends AuthState {
  isAuthenticated: boolean
  login: (token: string, displayName: string) => void
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)
