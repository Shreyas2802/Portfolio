import { Link, useParams } from 'react-router-dom'
import { DataLabel } from '../components/ui/DataLabel'
import { getProjectBySlug } from '../data/projects'

export function ProjectDetailPage() {
  const { slug } = useParams<{ slug: string }>()
  const project = slug ? getProjectBySlug(slug) : undefined

  if (!project) {
    return (
      <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
        <DataLabel items={['NOT FOUND']} />
        <h1 className="mt-6 text-4xl font-semibold tracking-tight">Project not found</h1>
        <Link to="/projects" className="data-label mt-6 inline-block text-[var(--color-brass)]">
          ← Back to Projects
        </Link>
      </section>
    )
  }

  return (
    <section className="mx-auto max-w-[1000px] px-6 py-24 sm:px-10">
      <Link to="/projects" className="data-label text-[var(--color-ink-muted)] hover:text-[var(--color-brass)]">
        ← Back to Projects
      </Link>

      <DataLabel items={[project.status]} />
      <h1 className="mt-4 text-4xl font-semibold tracking-tight sm:text-5xl">{project.title}</h1>
      <p className="mt-3 text-lg text-[var(--color-ink-muted)]">{project.tagline}</p>

      <div className="mt-8 flex flex-wrap gap-2">
        {project.tech.map((t) => (
          <span
            key={t}
            className="data-label rounded-full border border-[var(--color-line)] px-3 py-1"
          >
            {t}
          </span>
        ))}
      </div>

      <div className="mt-12 border-t border-[var(--color-line)] pt-12">
        <DataLabel items={['OVERVIEW']} />
        <p className="mt-4 text-[var(--color-ink)]">{project.overview}</p>
      </div>

      <div className="mt-12 border-t border-[var(--color-line)] pt-12">
        <DataLabel items={['ROLE']} />
        <p className="mt-4 text-[var(--color-ink)]">{project.role}</p>
      </div>

      <div className="mt-12 border-t border-[var(--color-line)] pt-12">
        <DataLabel items={['KEY CONTRIBUTIONS']} />
        <ul className="mt-4 flex flex-col gap-3">
          {project.contributions.map((point) => (
            <li
              key={point}
              className="pl-4 text-[var(--color-ink-muted)] before:float-left before:-ml-4 before:text-[var(--color-brass)] before:content-['—_']"
            >
              {point}
            </li>
          ))}
        </ul>
      </div>
    </section>
  )
}
