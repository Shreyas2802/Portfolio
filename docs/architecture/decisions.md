# Architecture Decisions Log

Record decisions here as they're made, in the format:

## YYYY-MM-DD — Short title
**Decision:** what was decided.
**Why:** the reasoning.
**Alternatives considered:** what else was on the table and why it lost.

---

## Phase 0 — Foundation
**Decision:** Monorepo with `apps/web` (Vite + React + TS) and
`apps/api` (ASP.NET Core, 4-project clean-architecture split).
**Why:** matches the spec's non-negotiable engineering requirements —
strong typing, clear separation of concerns, no business logic in
React components. A monorepo keeps frontend/backend changes reviewable
together without the overhead of managing two separate repos this
early.
**Alternatives considered:** Single ASP.NET Core project serving
Razor/React together (rejected — couples deploy lifecycles and makes
the "API is the trust boundary" principle harder to enforce). Separate
repos (rejected — unnecessary process overhead for a solo project at
this stage).

**Decision:** Tailwind CSS v4 via the `@tailwindcss/vite` plugin
instead of a `tailwind.config.js` + PostCSS setup.
**Why:** v4 is the current version as of this build and configures
itself through CSS (`@theme` in `index.css`) rather than a JS config
file — fewer moving parts, and it's what `npm install tailwindcss`
resolves to today.

**Decision:** ESLint (flat config) + Prettier instead of Vite's
default `oxlint` scaffold.
**Why:** the spec explicitly lists ESLint + Prettier under Tooling
(section 3). oxlint is fast but newer and not what was specified.

---

## Phase 1 — React foundation & design system
**Decision:** Neutral, near-black chrome (`#0B0B0D` canvas, warm
off-white text, one desaturated brass accent) rather than committing
the UI to one photographic mood (moody / airy / warm).
**Why:** the photography itself spans multiple genres, so a UI that
picked one mood would fight with half the portfolio. Neutral chrome
lets each photo/album carry its own mood; the site's identity comes
from restraint and the signature device below, not from a color story.

**Decision:** Camera/EXIF-style technical labels (`DataLabel`
component, JetBrains Mono, uppercase, tracked-out) used as a recurring
UI device — nav labels, section headers, hero eyebrow.
**Why:** this is the one visual idea that's actually specific to
"photographer + developer" rather than a generic portfolio template.
It's decorative/placeholder now; once Phase 4 wires up real
Photo.Aperture/FocalLength/etc, the same component displays real EXIF
data instead of static text.

**Decision:** Self-hosted fonts via `@fontsource` (Inter + JetBrains
Mono) instead of a Google Fonts `<link>`.
**Why:** no external font CDN request on every page load (matches the
spec's "fast first load" requirement and avoids leaking visitor
metadata to a third party).

**Decision:** `createBrowserRouter` from `react-router-dom` with a
shared `RootLayout` (header/footer) wrapping all routes.
**Why:** every page needs the same header/footer; centralizing it
avoids duplicating layout markup per-page, and keeps route-level
components focused on their own content only.

---

## Phase 2 — ASP.NET Core foundation (EF Core, SQL Server, error handling)
**Decision:** `SiteSettings` (from the spec's own domain model) as the
first real Domain entity, rather than jumping straight to Photo/Album.
**Why:** Photo and Album need image upload/processing (Phase 5) to be
meaningful - adding them now would mean a DbSet nothing populates.
SiteSettings is simple, already in the spec, and is enough to prove
Domain → Infrastructure → EF Core → SQL Server works end to end before
anything harder is layered on.

**Decision:** SQL Server LocalDB for local dev
(`(localdb)\MSSQLLocalDB`), trusted/Windows connection - no password
in the connection string.
**Why:** matches what's actually installed on this machine, and
Windows-integrated auth means there's no credential to accidentally
leak even though the connection string is committed. This connection
string is dev-only; a production connection string must come from
environment variables or a secrets store, never committed - see
`appsettings.Production.json` (gitignored) when that's introduced in
Phase 10.

**Decision:** `AddProblemDetails()` + `UseDeveloperExceptionPage()` in
dev / `UseExceptionHandler()` otherwise, added now rather than in
Phase 7 (Hardening).
**Why:** it's a few lines and costs nothing to have on from the start;
retrofitting error handling after controllers exist is more error-prone
than starting with it.
