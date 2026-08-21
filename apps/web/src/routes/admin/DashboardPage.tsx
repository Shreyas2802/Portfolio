import { Link } from 'react-router-dom'
import { DataLabel } from '../../components/ui/DataLabel'
import { useAdminStats } from '../../features/admin/useAdminStats'

export function DashboardPage() {
  const { data: stats, isLoading } = useAdminStats()

  return (
    <section className="mx-auto max-w-[1600px] px-6 py-16 sm:px-10">
      <DataLabel items={['DASHBOARD']} />
      <h1 className="mt-4 text-3xl font-semibold tracking-tight">Overview</h1>

      {isLoading && <p className="mt-8 text-[var(--color-ink-muted)]">Loading…</p>}

      {stats && (
        <div className="mt-8 grid grid-cols-2 gap-4 lg:grid-cols-4">
          <StatCard label="Photos" value={stats.photoCount} />
          <StatCard label="Albums" value={stats.albumCount} />
          <StatCard label="Projects" value={stats.projectCount} />
          <StatCard label="Unread Messages" value={stats.unreadContactMessageCount} />
        </div>
      )}

      <div className="mt-12">
        <Link
          to="/admin/photos"
          className="rounded-full bg-[var(--color-ink)] px-6 py-3 text-sm font-medium text-[var(--color-canvas)] transition-opacity hover:opacity-90"
        >
          Manage Photos
        </Link>
      </div>
    </section>
  )
}

function StatCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-lg border border-[var(--color-line)] p-6">
      <p className="data-label">{label}</p>
      <p className="mt-2 text-3xl font-semibold">{value}</p>
    </div>
  )
}
