export interface Project {
  slug: string
  title: string
  tagline: string
  role: string
  status: string
  tech: string[]
  overview: string
  contributions: string[]
}

/**
 * Static project data for Phase 3. This becomes an API-backed
 * Project entity in a later phase if/when the admin CMS needs to
 * edit these without a redeploy - not necessary yet for two
 * hand-written case studies.
 */
export const FEATURED_PROJECTS: Project[] = [
  {
    slug: 'brahmanda-tatva',
    title: 'Brahmanda Tatva',
    tagline: 'Multi-tenant authentication & authorization platform',
    role: 'Requirements, UI implementation, and QA',
    status: 'MVP reached — additional modules planned before commercial launch',
    tech: ['.NET 9', 'ASP.NET Core MVC', 'EF Core 9', 'SQL Server', 'Postman'],
    overview:
      'An internal platform handling authentication, role-based access control, tenant provisioning, and invitation workflows across multiple tenants and products. Built from early development through MVP.',
    contributions: [
      'Defined functional requirements and behavior specifications for authentication, role-based access control, tenant provisioning, and invitation workflows, working directly with the development team to translate them into technical specs.',
      "Implemented UI screens across multiple product modules by adapting and replicating the platform's design system, keeping visual and functional behavior consistent across tenants.",
      'Owned functional and validation testing for authentication, authorization, and tenant-management flows using Postman, identifying issues ahead of release.',
      'Drove the project day-to-day with architectural guidance and test-scenario input from a senior developer.',
    ],
  },
  {
    slug: 'portfolio-platform',
    title: 'Portfolio Platform',
    tagline: 'This site — photography portfolio, dev portfolio, and admin CMS',
    role: 'Architecture, full-stack implementation, and design',
    status: 'In active development, built phase by phase',
    tech: ['React', 'TypeScript', 'Vite', 'Tailwind CSS', 'ASP.NET Core', 'EF Core', 'SQL Server'],
    overview:
      'A full-stack site combining a photography portfolio, a software-development portfolio, and a private admin CMS — built as a clean-architecture monorepo (Domain / Application / Infrastructure / Api on the backend, a typed React SPA on the frontend) to be a real, working demonstration of both skill sets rather than a template.',
    contributions: [
      'Designed the monorepo and clean-architecture project structure, keeping business logic out of both the database layer and the UI.',
      'Built the design system from scratch — palette, typography, and a signature EXIF-style technical-label device tying the photography and engineering sides of the site together.',
      'Wired up EF Core, SQL Server migrations, and a contact form backed by a real database table with server-side validation and honeypot spam protection.',
      'Following a phased build plan (foundation → design system → backend → content → photography → admin → hardening → deployment), verifying each phase locally before moving to the next.',
    ],
  },
]

export function getProjectBySlug(slug: string): Project | undefined {
  return FEATURED_PROJECTS.find((p) => p.slug === slug)
}
