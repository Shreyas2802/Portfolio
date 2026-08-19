/**
 * Thin fetch wrapper for talking to the ASP.NET Core API.
 *
 * Why this exists: every request should go through one place so that
 * base URL, error handling, and (later) auth headers live in exactly
 * one spot instead of being copy-pasted into every component. Nothing
 * in here is React-specific — components should never call fetch()
 * directly (see architectural principle #7/#8 in the spec: business
 * logic and API concerns stay out of presentation components).
 */

export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export async function apiGet<T>(path: string): Promise<T> {
  const res = await fetch(`/api${path}`)
  if (!res.ok) {
    throw new ApiError(`GET ${path} failed`, res.status)
  }
  return (await res.json()) as T
}
