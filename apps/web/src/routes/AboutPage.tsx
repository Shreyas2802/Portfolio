import { DataLabel } from '../components/ui/DataLabel'

const EXPERIENCE = [
  {
    role: 'Junior Software Developer',
    org: 'Vistrut Technologies',
    period: 'Apr 2025 — Present',
    points: [
      'Build and maintain responsive front-end interfaces using Tailwind CSS, Bootstrap, and JavaScript across internal and client-facing applications.',
      'Define functional requirements and test authentication/authorization workflows for a multi-tenant platform, validating role-based access control and session behavior across products and tenants.',
      'Validate backend APIs and integration points using Swagger and Postman, identifying and reporting issues before release.',
      'Deploy and manage updates to live production sites via FileZilla and Azure.',
    ],
  },
  {
    role: 'Software Development Intern',
    org: 'Vistrut Technologies',
    period: 'Oct 2024 — Apr 2025',
    points: [
      'Redesigned and optimized a client website, improving UI/UX and responsiveness.',
      'Integrated the Razorpay payment gateway and FullCalendar for event management.',
      'Worked directly with the client to implement new features and resolve reported bugs.',
      'Deployed websites to live servers and managed client-side updates.',
    ],
  },
] as const

const SKILLS = {
  Frontend: ['HTML', 'CSS', 'Tailwind CSS', 'Bootstrap 5', 'JavaScript', 'React', 'TypeScript'],
  Backend: ['C#', 'ASP.NET Core MVC', 'ASP.NET Core Web API', 'JWT Authentication'],
  Database: ['SQL Server', 'Entity Framework Core'],
  Tools: ['Git', 'Swagger', 'Postman', 'FileZilla', 'Azure'],
} as const

const EDUCATION = [
  {
    degree: 'B.Tech, Computer Engineering',
    school: 'Ajeenkya DY Patil College of Engineering',
    period: 'Nov 2021 — May 2024',
    detail: 'CGPA: 7.24',
  },
  {
    degree: 'Diploma, Computer Engineering',
    school: 'MIT Polytechnic',
    period: '2017 — 2021',
    detail: '90.29%',
  },
] as const

export function AboutPage() {
  return (
    <>
      <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
        <DataLabel items={['ABOUT']} />
        <h1 className="mt-6 max-w-3xl text-4xl font-semibold leading-tight tracking-tight sm:text-5xl">
          Full-stack .NET developer, and a photographer before that.
        </h1>
        <p className="mt-6 max-w-2xl text-lg text-[var(--color-ink-muted)]">
          I work across ASP.NET Core Web APIs, SQL Server, and authentication/authorization
          systems, plus front-end development, deployment, and QA — the full lifecycle of
          getting software into production. I'm currently contributing to the requirements,
          UI implementation, and testing of a multi-tenant authentication platform, from
          early development through MVP.
        </p>
        <p className="mt-4 max-w-2xl text-[var(--color-ink-muted)]">
          Before any of that, I was Photography Committee Head at MIT Polytechnic and
          Ajeenkya DY Patil University — leading event photography and coordinating student
          volunteers through academic and cultural events. This site is where both sides
          live together.
        </p>
        <a
          href="/documents/Shreyas_Gune_Resume.pdf"
          target="_blank"
          rel="noreferrer"
          className="mt-8 inline-block rounded-full bg-[var(--color-ink)] px-6 py-3 text-sm font-medium text-[var(--color-canvas)] transition-opacity hover:opacity-90"
        >
          Download Resume (PDF)
        </a>
      </section>

      <section className="border-t border-[var(--color-line)] px-6 py-24 sm:px-10">
        <div className="mx-auto max-w-[1600px]">
          <DataLabel items={['EXPERIENCE']} />
          <div className="mt-8 flex flex-col gap-12">
            {EXPERIENCE.map((job) => (
              <div key={job.role} className="grid gap-2 sm:grid-cols-[1fr_2fr] sm:gap-8">
                <div>
                  <p className="font-medium text-[var(--color-ink)]">{job.role}</p>
                  <p className="text-sm text-[var(--color-ink-muted)]">{job.org}</p>
                  <p className="data-label mt-1">{job.period}</p>
                </div>
                <ul className="flex flex-col gap-2 text-[var(--color-ink-muted)]">
                  {job.points.map((point) => (
                    <li key={point} className="pl-4 before:float-left before:-ml-4 before:text-[var(--color-brass)] before:content-['—_']">
                      {point}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="border-t border-[var(--color-line)] px-6 py-24 sm:px-10">
        <div className="mx-auto max-w-[1600px]">
          <DataLabel items={['SKILLS']} />
          <div className="mt-8 grid gap-8 sm:grid-cols-2 lg:grid-cols-4">
            {Object.entries(SKILLS).map(([category, items]) => (
              <div key={category}>
                <p className="data-label mb-3">{category}</p>
                <ul className="flex flex-col gap-1 text-[var(--color-ink-muted)]">
                  {items.map((item) => (
                    <li key={item}>{item}</li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="border-t border-[var(--color-line)] px-6 py-24 sm:px-10">
        <div className="mx-auto max-w-[1600px]">
          <DataLabel items={['EDUCATION']} />
          <div className="mt-8 grid gap-8 sm:grid-cols-2">
            {EDUCATION.map((ed) => (
              <div key={ed.degree}>
                <p className="font-medium text-[var(--color-ink)]">{ed.degree}</p>
                <p className="text-sm text-[var(--color-ink-muted)]">{ed.school}</p>
                <p className="data-label mt-1">
                  {ed.period} · {ed.detail}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>
    </>
  )
}
