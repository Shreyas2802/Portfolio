import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { apiPost, ApiError } from '../../lib/api'
import { useAuth } from '../../features/auth/useAuth'

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
    <div className="flex min-h-screen items-center justify-center bg-[var(--color-canvas)] px-6">
      <div className="w-full max-w-sm">

        {/* Logo mark */}
        <div className="mb-8 flex items-center gap-2">
          <span className="h-2 w-2 rounded-full bg-[var(--color-gold)]" aria-hidden="true" />
          <span className="font-semibold tracking-tight text-[var(--color-ink)]">Shreyas</span>
          <span className="data-label ml-1 text-[var(--color-ink-faint)]">/ admin</span>
        </div>

        {/* Card */}
        <div className="rounded-xl border border-[var(--color-line)] bg-[var(--color-canvas-raised)] p-8">
          <h1 className="text-xl font-semibold tracking-tight text-[var(--color-ink)]">Sign in</h1>
          <p className="mt-1 text-sm text-[var(--color-ink-muted)]">
            Admin access only.
          </p>

          <form onSubmit={handleSubmit} className="mt-7 flex flex-col gap-5">
            <div className="flex flex-col gap-1.5">
              <label htmlFor="username" className="data-label text-[var(--color-ink-muted)]">
                Username
              </label>
              <input
                id="username"
                type="text"
                autoComplete="username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className="rounded-lg border border-[var(--color-line)] bg-[var(--color-canvas)] px-4 py-2.5 text-sm text-[var(--color-ink)] outline-none transition-colors placeholder:text-[var(--color-ink-faint)] focus:border-[var(--color-gold)]"
                placeholder="your username"
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="password" className="data-label text-[var(--color-ink-muted)]">
                Password
              </label>
              <input
                id="password"
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="rounded-lg border border-[var(--color-line)] bg-[var(--color-canvas)] px-4 py-2.5 text-sm text-[var(--color-ink)] outline-none transition-colors placeholder:text-[var(--color-ink-faint)] focus:border-[var(--color-gold)]"
                placeholder="••••••••"
              />
            </div>

            {mutation.isError && (
              <p className="rounded-lg border border-red-900/40 bg-red-950/30 px-4 py-2.5 text-sm text-red-400" role="alert">
                {mutation.error instanceof ApiError && mutation.error.status === 401
                  ? 'Invalid username or password.'
                  : 'Something went wrong. Please try again.'}
              </p>
            )}

            <button
              type="submit"
              disabled={mutation.isPending || !username || !password}
              className="mt-1 rounded-lg bg-[var(--color-gold)] px-6 py-2.5 text-sm font-semibold text-[var(--color-canvas)] transition-all hover:bg-[var(--color-gold-dim)] disabled:cursor-not-allowed disabled:opacity-50"
            >
              {mutation.isPending ? 'Signing in…' : 'Sign in'}
            </button>
          </form>
        </div>

        {/* Back to site */}
        <p className="mt-6 text-center">
          <a
            href="/"
            className="data-label text-[var(--color-ink-faint)] transition-colors hover:text-[var(--color-gold)]"
          >
            ← Back to site
          </a>
        </p>
      </div>
    </div>
  )
}
