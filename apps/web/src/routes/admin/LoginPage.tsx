import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { apiPost, ApiError } from '../../lib/api'
import { useAuth } from '../../features/auth/useAuth'
import { DataLabel } from '../../components/ui/DataLabel'

interface LoginResponse {
  token: string
  expiresAtUtc: string
  displayName: string
}

export function LoginPage() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const { login } = useAuth()
  const navigate = useNavigate()

  const mutation = useMutation({
    mutationFn: (payload: { username: string; password: string }) =>
      apiPost<typeof payload, LoginResponse>('/auth/login', payload),
    onSuccess: (data) => {
      login(data.token, data.displayName)
      navigate('/admin')
    },
  })

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    mutation.mutate({ username, password })
  }

  return (
    <div className="flex min-h-screen items-center justify-center px-6">
      <form onSubmit={handleSubmit} className="w-full max-w-sm">
        <DataLabel items={['ADMIN']} />
        <h1 className="mt-4 text-2xl font-semibold tracking-tight">Sign in</h1>

        <div className="mt-8 flex flex-col gap-4">
          <div className="flex flex-col gap-2">
            <label htmlFor="username" className="data-label">
              Username
            </label>
            <input
              id="username"
              type="text"
              autoComplete="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="rounded-md border border-[var(--color-line)] bg-transparent px-4 py-3 text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
            />
          </div>
          <div className="flex flex-col gap-2">
            <label htmlFor="password" className="data-label">
              Password
            </label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="rounded-md border border-[var(--color-line)] bg-transparent px-4 py-3 text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
            />
          </div>

          {mutation.isError && (
            <p className="text-sm text-red-400" role="alert">
              {mutation.error instanceof ApiError && mutation.error.status === 401
                ? 'Invalid username or password.'
                : 'Something went wrong. Please try again.'}
            </p>
          )}

          <button
            type="submit"
            disabled={mutation.isPending}
            className="mt-2 rounded-full bg-[var(--color-ink)] px-6 py-3 text-sm font-medium text-[var(--color-canvas)] transition-opacity hover:opacity-90 disabled:opacity-50"
          >
            {mutation.isPending ? 'Signing in…' : 'Sign in'}
          </button>
        </div>
      </form>
    </div>
  )
}
