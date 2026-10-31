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
      'Redesigned and optimised a client website, improving UI/UX and responsiveness.',
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
    detail: 'CGPA 7.24',
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
      {/* ── Intro ──────────────────────────────────────────────────── */}
      <section className="mx-auto max-w-[1600px] px-6 py-24 sm:px-10">
        <h1 className="max-w-3xl text-4xl font-semibold leading-[1.12] tracking-tight text-[var(--color-ink)] sm:text-5xl">
          Full-stack .NET developer,
          <br className="hidden sm:block" /> and photographer before that.
        </h1>

        <div className="mt-8 max-w-2xl space-y-4 text-[1.0625rem] leading-relaxed text-[var(--color-ink-muted)]">
          <p>
            I work across ASP.NET Core APIs, SQL Server, and auth systems — through
            to front-end, deployment, and QA. The full lifecycle of getting software
            into production, not just one layer of it. Right now I'm contributing
            to the requirements, UI, and testing of a multi-tenant authentication
            platform at Vistrut Technologies.
          </p>
          <p>
            Before any of that, I was Photography Committee Head at MIT Polytechnic
            and Ajeenkya DY Patil University — leading event photography and
            coordinating volunteers through academic and cultural events. This site
            is where both sides live together.
          </p>
        </div>

        <a
          href="/documents/Shreyas_Gune_Resume.pdf"
          target="_blank"
          rel="noreferrer"
          className="mt-8 inline-flex items-center gap-2 rounded-full bg-[var(--color-ink)] px-6 py-3 text-sm font-semibold text-[var(--color-canvas)] transition-opacity hover:opacity-85"
        >
          Download Resume
        </a>
      </section>

      {/* ── Experience ─────────────────────────────────────────────── */}
      <section className="border-t border-[var(--color-line)] px-6 py-20 sm:px-10">
        <div className="mx-auto max-w-[1600px]">
          <h2 className="text-xs font-medium uppercase tracking-widest text-[var(--color-ink-faint)]">
            Experience
          </h2>
          <div className="mt-10 flex flex-col divide-y divide-[var(--color-line)]">
            {EXPERIENCE.map((job) => (
              <div key={job.role} className="grid gap-6 py-10 first:pt-0 last:pb-0 sm:grid-cols-[200px_1fr] sm:gap-12">
                <div className="pt-0.5">
                  <p className="text-sm font-semibold text-[var(--color-ink)]">{job.role}</p>
                  <p className="mt-1 text-sm text-[var(--color-ink-muted)]">{job.org}</p>
                  <p className="mt-1 font-mono text-[0.625rem] uppercase tracking-widest text-[var(--color-ink-faint)]">
                    {job.period}
                  </p>
                </div>
                <ul className="space-y-2.5">
                  {job.points.map((point) => (
                    <li
                      key={point}
                      className="flex gap-3 text-[0.9375rem] leading-relaxed text-[var(--color-ink-muted)]"
                    >
                      <span className="mt-[0.4em] h-1 w-1 shrink-0 rounded-full bg-[var(--color-gold)]" aria-hidden="true" />
                      {point}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ── Skills ─────────────────────────────────────────────────── */}
      <section className="section-alt border-t border-[var(--color-line)] px-6 py-20 sm:px-10">
        <div className="mx-auto max-w-[1600px]">
          <h2 className="text-xs font-medium uppercase tracking-widest text-[var(--color-ink-faint)]">
            Skills
          </h2>
          <div className="mt-10 grid gap-8 sm:grid-cols-2 lg:grid-cols-4">
            {Object.entries(SKILLS).map(([category, items]) => (
              <div key={category}>
                <p className="text-sm font-semibold text-[var(--color-ink)]">{category}</p>
                <ul className="mt-3 space-y-1.5">
                  {items.map((item) => (
                    <li key={item} className="text-sm text-[var(--color-ink-muted)]">
                      {item}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ── Education ──────────────────────────────────────────────── */}
      <section className="border-t border-[var(--color-line)] px-6 py-20 sm:px-10">
        <div className="mx-auto max-w-[1600px]">
          <h2 className="text-xs font-medium uppercase tracking-widest text-[var(--color-ink-faint)]">
            Education
          </h2>
          <div className="mt-10 grid gap-8 sm:grid-cols-2">
            {EDUCATION.map((ed) => (
              <div key={ed.degree}>
                <p className="text-[0.9375rem] font-semibold text-[var(--color-ink)]">{ed.degree}</p>
                <p className="mt-1 text-sm text-[var(--color-ink-muted)]">{ed.school}</p>
                <p className="mt-1 font-mono text-[0.625rem] uppercase tracking-widest text-[var(--color-ink-faint)]">
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
