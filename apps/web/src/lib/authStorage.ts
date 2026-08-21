export interface AuthState {
  token: string | null
  displayName: string | null
}

const STORAGE_KEY = 'portfolio-admin-auth'

export function loadStoredAuth(): AuthState {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return { token: null, displayName: null }
    return JSON.parse(raw) as AuthState
  } catch {
    return { token: null, displayName: null }
  }
}

export function saveStoredAuth(state: AuthState): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(state))
}

export function clearStoredAuth(): void {
  localStorage.removeItem(STORAGE_KEY)
}

/** For use outside React components (e.g. the api.ts fetch wrapper). */
export function getStoredToken(): string | null {
  return loadStoredAuth().token
}
