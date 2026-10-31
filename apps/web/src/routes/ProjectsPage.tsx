import { Link } from 'react-router-dom'
import { useProjects } from '../features/projects/usePublicProjects'

export function ProjectsPage() {
  const { data: projects, isLoading } = useProjects()
  const featured  = projects?.filter((p) => p.isFeatured) ?? []
  const otherWork = projects?.filter((p) => !p.isFeatured) ?? []

  return (
    <>
      <section aria-label="Featured projects" className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
        <h1 className="text-4xl font-semibold tracking-tight text-[var(--color-ink)]">Projects</h1>
        <p className="mt-4 max-w-xl text-[var(--color-ink-muted)]">
          Two projects, front and center: a work platform built for a real business, and
          this site — built to show how I actually think through a full-stack build.
        </p>

        {isLoading && (
          <p className="mt-12 text-[var(--color-ink-muted)]">Loading…</p>
        )}

        {featured.length > 0 && (
          <div className="mt-12 grid gap-6 lg:grid-cols-2">
            {featured.map((project) => (
              <Link
                key={project.slug}
                to={`/projects/${project.slug}`}
                className="group flex flex-col justify-between rounded-lg border border-[var(--color-line)] p-8 transition-colors hover:border-[var(--color-gold)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-gold)]"
              >
                <div>
                  <p className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-muted)]">
                    {project.status.split('—')[0].trim()}
                  </p>
                  <h2 className="mt-4 text-2xl font-semibold tracking-tight text-[var(--color-ink)]">
                    {project.title}
                  </h2>
                  <p className="mt-2 text-[var(--color-ink-muted)]">{project.tagline}</p>
                </div>
                <p className="mt-8 font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-gold)] transition-transform group-hover:translate-x-1">
                  View case study →
                </p>
              </Link>
            ))}
          </div>
        )}
      </section>

      {otherWork.length > 0 && (
        <section aria-label="Other work" className="border-t border-[var(--color-line)] px-6 py-24 sm:px-10">
          <div className="mx-auto max-w-[1600px]">
            <h2 className="text-xs font-medium uppercase tracking-widest text-[var(--color-ink-faint)]">
              Other work
            </h2>
            <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {otherWork.map((project) => (
                /* Now a Link — consistent with featured cards */
                <Link
                  key={project.slug}
                  to={`/projects/${project.slug}`}
                  className="rounded-lg border border-[var(--color-line)] p-6 transition-colors hover:border-[var(--color-gold)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-gold)]"
                >
                  <h3 className="font-medium text-[var(--color-ink)]">{project.title}</h3>
                  <p className="mt-2 text-sm text-[var(--color-ink-muted)]">{project.tagline}</p>
                  <p className="mt-4 font-mono text-[0.625rem] uppercase tracking-widest text-[var(--color-ink-faint)]">
                    {project.technologies.join(' · ')}
                  </p>
                </Link>
              ))}
            </div>
          </div>
        </section>
      )}
    </>
  )
}
