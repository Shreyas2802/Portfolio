import { Link } from 'react-router-dom'
import { DataLabel } from '../components/ui/DataLabel'
import { FEATURED_PROJECTS } from '../data/projects'

const OTHER_WORK = [
  {
    title: 'AuthServer Authentication Platform',
    blurb: 'Internal authentication server for user login and access management. Tested JWT-based auth workflows with Swagger and Postman.',
    tech: '.NET 8 · ASP.NET Core Web API · SQL Server · JWT · Swagger',
  },
  {
    title: 'Jyoti Ashram Website',
    blurb: 'Responsive client website for managing events, donations, and public information, with FullCalendar and Razorpay integration.',
    tech: '.NET 7 · ASP.NET Core MVC · SQL Server · Tailwind CSS · Bootstrap · Razorpay · Azure',
  },
  {
    title: 'Vistrut Task Tracker',
    blurb: 'Internal task management application for workflow and task tracking across teams.',
    tech: '.NET 7 · ASP.NET Core MVC · SQL Server · Bootstrap · Knockout.js',
  },
] as const

export function ProjectsPage() {
  return (
    <>
      <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
        <DataLabel items={['PROJECTS']} />
        <h1 className="mt-6 text-4xl font-semibold tracking-tight">Projects</h1>
        <p className="mt-4 max-w-xl text-[var(--color-ink-muted)]">
          Two projects, front and center: a work platform built for a real business, and
          this site — built to show how I actually think through a full-stack build.
        </p>

        <div className="mt-12 grid gap-6 lg:grid-cols-2">
          {FEATURED_PROJECTS.map((project) => (
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
      </section>

      <section className="border-t border-[var(--color-line)] px-6 py-24 sm:px-10">
        <div className="mx-auto max-w-[1600px]">
          <DataLabel items={['OTHER WORK']} />
          <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {OTHER_WORK.map((item) => (
              <div key={item.title} className="rounded-lg border border-[var(--color-line)] p-6">
                <h3 className="font-medium text-[var(--color-ink)]">{item.title}</h3>
                <p className="mt-2 text-sm text-[var(--color-ink-muted)]">{item.blurb}</p>
                <p className="data-label mt-4">{item.tech}</p>
              </div>
            ))}
          </div>
        </div>
      </section>
    </>
  )
}
