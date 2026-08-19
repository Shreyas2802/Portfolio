import { useHealthCheck } from './features/health/useHealthCheck'

/**
 * Phase 0 shell. This is intentionally not the real homepage — the
 * spec's "First Codex Task" (section 51) says explicitly not to build
 * the full UI yet. This screen exists only to prove the SPA can reach
 * the API through the dev proxy and that the build/type-check/lint
 * pipeline works end to end. Phase 1 replaces this with the real
 * design system and homepage skeleton.
 */
function App() {
  const { data, isLoading, isError, error } = useHealthCheck()

  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-4 px-6 text-center">
      <h1 className="font-[var(--font-display)] text-3xl font-semibold tracking-tight">
        Portfolio Platform — Foundation
      </h1>
      <p className="max-w-md text-neutral-400">
        Phase 0 shell. This page's only job is to confirm the React app
        can reach the ASP.NET Core API.
      </p>

      <div className="mt-4 rounded-lg border border-neutral-800 bg-neutral-900 px-6 py-4 text-sm">
        {isLoading && <span className="text-neutral-400">Checking API health…</span>}
        {isError && (
          <span className="text-red-400">
            API unreachable ({error instanceof Error ? error.message : 'unknown error'}).
            Start the API with <code className="text-red-300">dotnet run</code> in{' '}
            <code className="text-red-300">apps/api/Portfolio.Api</code>.
          </span>
        )}
        {data && (
          <span className="text-emerald-400">
            API status: {data.status} · {new Date(data.timestampUtc).toLocaleTimeString()}
          </span>
        )}
      </div>
    </main>
  )
}

export default App
