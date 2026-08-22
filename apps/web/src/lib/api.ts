/**
 * Thin fetch wrapper for talking to the ASP.NET Core API.
 *
 * Why this exists: every request should go through one place so that
 * base URL, error handling, and auth headers live in exactly one spot
 * instead of being copy-pasted into every component. Nothing in here
 * is React-specific — components should never call fetch() directly
 * (see architectural principle #7/#8 in the spec: business logic and
 * API concerns stay out of presentation components).
 */
import { getStoredToken } from './authStorage'

export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

function authHeaders(): HeadersInit {
  const token = getStoredToken()
  return token ? { Authorization: `Bearer ${token}` } : {}
}

export async function apiGet<T>(path: string): Promise<T> {
  const res = await fetch(`/api${path}`)
  if (!res.ok) {
    throw new ApiError(`GET ${path} failed`, res.status)
  }
  return (await res.json()) as T
}

export async function apiPost<TBody, TResponse>(path: string, body: TBody): Promise<TResponse> {
  const res = await fetch(`/api${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) {
    throw new ApiError(`POST ${path} failed`, res.status)
  }
  return (await res.json()) as TResponse
}

/** Authenticated GET - attaches the admin's JWT if present. */
export async function apiAuthGet<T>(path: string): Promise<T> {
  const res = await fetch(`/api${path}`, { headers: authHeaders() })
  if (!res.ok) {
    throw new ApiError(`GET ${path} failed`, res.status)
  }
  return (await res.json()) as T
}

/** Authenticated JSON POST - attaches the admin's JWT if present. */
export async function apiAuthPostJson<TBody, TResponse>(path: string, body: TBody): Promise<TResponse> {
  const res = await fetch(`/api${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(body),
  })
  if (!res.ok) {
    throw new ApiError(`POST ${path} failed`, res.status)
  }
  return (await res.json()) as TResponse
}

/** Authenticated JSON PUT - attaches the admin's JWT if present. */
export async function apiAuthPutJson<TBody, TResponse>(path: string, body: TBody): Promise<TResponse> {
  const res = await fetch(`/api${path}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(body),
  })
  if (!res.ok) {
    throw new ApiError(`PUT ${path} failed`, res.status)
  }
  return (await res.json()) as TResponse
}

/** Authenticated multipart/form-data POST - for the photo upload endpoint. */
export async function apiAuthPostForm<T>(path: string, formData: FormData): Promise<T> {
  const res = await fetch(`/api${path}`, {
    method: 'POST',
    headers: authHeaders(),
    body: formData,
  })
  if (!res.ok) {
    let message = `POST ${path} failed`
    try {
      const body = await res.json()
      if (body?.error) message = body.error
    } catch {
      // response wasn't JSON - keep the generic message
    }
    throw new ApiError(message, res.status)
  }
  return (await res.json()) as T
}

/** Authenticated DELETE - attaches the admin's JWT if present. */
export async function apiAuthDelete(path: string): Promise<void> {
  const res = await fetch(`/api${path}`, { method: 'DELETE', headers: authHeaders() })
  if (!res.ok) {
    throw new ApiError(`DELETE ${path} failed`, res.status)
  }
}
