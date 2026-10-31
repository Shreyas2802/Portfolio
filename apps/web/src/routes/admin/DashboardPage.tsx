import { Link } from 'react-router-dom'
import { DataLabel } from '../../components/ui/DataLabel'
import { useAdminStats } from '../../features/admin/useAdminStats'

export function DashboardPage() {
  const { data: stats, isLoading } = useAdminStats()

  return (
    <section className="mx-auto max-w-[1600px] px-6 py-12 sm:px-10">
      <DataLabel items={['DASHBOARD']} />
      <h1 className="mt-3 text-3xl font-semibold tracking-tight text-[var(--color-ink)]">
        Overview
      </h1>

      {isLoading && (
        <div className="mt-8 grid grid-cols-2 gap-4 lg:grid-cols-4">
          {[1, 2, 3, 4].map((i) => (
            <div
              key={i}
              className="h-28 animate-pulse rounded-xl border border-[var(--color-line)] bg-[var(--color-canvas-raised)]"
            />
          ))}
        </div>
      )}

      {stats && (
        <div className="mt-8 grid grid-cols-2 gap-4 lg:grid-cols-4">
          <StatCard label="Photos" value={stats.photoCount} accent />
          <StatCard label="Albums" value={stats.albumCount} />
          <StatCard label="Projects" value={stats.projectCount} />
          <StatCard
            label="Unread Messages"
            value={stats.unreadContactMessageCount}
            highlight={stats.unreadContactMessageCount > 0}
          />
        </div>
      )}

      {/* Quick actions */}
      <div className="mt-12">
        <DataLabel items={['QUICK ACTIONS']} />
        <div className="mt-4 flex flex-wrap gap-3">
          <Link
            to="/admin/photos"
            className="rounded-lg bg-[var(--color-gold)] px-5 py-2.5 text-sm font-semibold text-[var(--color-canvas)] transition-all hover:bg-[var(--color-gold-dim)]"
          >
            Upload Photos
          </Link>
          <Link
            to="/admin/albums"
            className="data-label rounded-lg border border-[var(--color-line)] px-5 py-2.5 text-[var(--color-ink-muted)] transition-colors hover:border-[var(--color-gold)] hover:text-[var(--color-gold)]"
          >
            Manage Albums
          </Link>
          <Link
            to="/admin/projects"
            className="data-label rounded-lg border border-[var(--color-line)] px-5 py-2.5 text-[var(--color-ink-muted)] transition-colors hover:border-[var(--color-gold)] hover:text-[var(--color-gold)]"
          >
            Manage Projects
          </Link>
          <Link
            to="/admin/taxonomy"
            className="data-label rounded-lg border border-[var(--color-line)] px-5 py-2.5 text-[var(--color-ink-muted)] transition-colors hover:border-[var(--color-gold)] hover:text-[var(--color-gold)]"
          >
            Categories & Tags
          </Link>
        </div>
      </div>
    </section>
  )
}

function StatCard({
  label,
  value,
  accent = false,
  highlight = false,
}: {
  label: string
  value: number
  accent?: boolean
  highlight?: boolean
}) {
  return (
    <div
      className={`rounded-xl border p-6 transition-colors ${
        highlight
          ? 'border-red-900/50 bg-red-950/20'
          : accent
          ? 'border-[var(--color-gold)]/30 bg-[var(--color-canvas-raised)]'
          : 'border-[var(--color-line)] bg-[var(--color-canvas-raised)]'
      }`}
    >
      <p className="data-label text-[var(--color-ink-muted)]">{label}</p>
      <p
        className={`mt-3 text-4xl font-semibold tracking-tight ${
          highlight
            ? 'text-red-400'
            : accent
            ? 'text-[var(--color-gold)]'
            : 'text-[var(--color-ink)]'
        }`}
      >
        {value}
      </p>
    </div>
  )
}
