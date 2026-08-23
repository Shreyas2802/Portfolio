import { Link } from 'react-router-dom'
import { DataLabel } from '../components/ui/DataLabel'
import { useProjects } from '../features/projects/usePublicProjects'

export function ProjectsPage() {
  const { data: projects, isLoading } = useProjects()
  const featured = projects?.filter((p) => p.isFeatured) ?? []
  const otherWork = projects?.filter((p) => !p.isFeatured) ?? []

  return (
    <>
      <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
        <DataLabel items={['PROJECTS']} />
        <h1 className="mt-6 text-4xl font-semibold tracking-tight">Projects</h1>
        <p className="mt-4 max-w-xl text-[var(--color-ink-muted)]">
          Two projects, front and center: a work platform built for a real business, and
          this site — built to show how I actually think through a full-stack build.
        </p>

        {isLoading && <p className="mt-12 text-[var(--color-ink-muted)]">Loading…</p>}

        {featured.length > 0 && (
          <div className="mt-12 grid gap-6 lg:grid-cols-2">
            {featured.map((project) => (
              <Link
                key={project.slug}
                to={`/projects/${project.slug}`}
                className="group flex flex-col justify-between rounded-lg border border-[var(--color-line)] p-8 transition-colors hover:border-[var(--color-brass)]"
              >
                <div>
                  <DataLabel items={[project.status.split('—')[0].trim()]} />
                  <h2 className="mt-4 text-2xl font-semibold tracking-tight">{project.title}</h2>
                  <p className="mt-2 text-[var(--color-ink-muted)]">{project.tagline}</p>
                </div>
                <p className="data-label mt-8 text-[var(--color-brass)] transition-transform group-hover:translate-x-1">
                  View case study →
                </p>
              </Link>
            ))}
          </div>
        )}
      </section>

      {otherWork.length > 0 && (
        <section className="border-t border-[var(--color-line)] px-6 py-24 sm:px-10">
          <div className="mx-auto max-w-[1600px]">
            <DataLabel items={['OTHER WORK']} />
            <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {otherWork.map((project) => (
                <div key={project.slug} className="rounded-lg border border-[var(--color-line)] p-6">
                  <h3 className="font-medium text-[var(--color-ink)]">{project.title}</h3>
                  <p className="mt-2 text-sm text-[var(--color-ink-muted)]">{project.tagline}</p>
                  <p className="data-label mt-4">{project.technologies.join(' · ')}</p>
                </div>
              ))}
            </div>
          </div>
        </section>
      )}
    </>
  )
}
