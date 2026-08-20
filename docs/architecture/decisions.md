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

---

## Phase 3 — Public portfolio content
**Decision:** Contact form is wired to a real `POST /api/contact`
endpoint backed by a `ContactMessage` table, not a fake form or a
`mailto:` link.
**Why:** the EF Core/migrations pipeline from Phase 2 was already
proven working - a static form would be a step backward, and a real
persisted contact message is exactly the kind of full-stack proof
point this site exists to demonstrate.

**Decision:** A honeypot field (`Website`, hidden via CSS + `aria-hidden`
+ `tabIndex={-1}`) rather than a CAPTCHA or third-party spam service.
**Why:** cheap, no dependency, no user friction, and catches the bulk
of naive bots. Real rate limiting is still tracked for Phase 7
(Hardening) - this is a first line of defense, not the final one.

**Decision:** Project case studies (Brahmanda Tatva, this Portfolio
Platform) are hand-written static data (`src/data/projects.ts`), not
API-backed.
**Why:** two case studies that change rarely don't justify a
`Project` DB table + admin CRUD yet. If/when the admin CMS (Phase 6)
needs to edit these without a redeploy, this is the natural place to
convert to an API call - the page components don't need to change.

**Decision:** Resume served as a static PDF from `public/documents/`
rather than a database-backed download.
**Why:** it's one file that changes infrequently; a static asset is
simpler than adding upload/versioning machinery for something this
low-frequency.

---

## Phase 4 — Photography (Photo/Album/Category, gallery, lightbox)
**Decision:** Real photos (11 shots from a Ganesh Chaturthi night
shooting Pune's well-known Ganpati mandals - Kasba Ganapati, Tambdi
Jogeshwari, Dagdusheth Halwai, and others) with real extracted EXIF
(Sony A6000, two lenses), seeded via a dev-only `DbSeeder`, instead of
placeholder images.
**Why:** the point of this site is to be a real demonstration, not a
mockup - and the DataLabel device is only meaningful if the numbers it
shows are real.

**Decision:** Hand-generated thumb/medium/large JPEG variants
(480/1200/2400px), stripped of EXIF on the served files, checked into
`wwwroot/uploads/photos/` - rather than building the full upload/
variant-generation pipeline now.
**Why:** that pipeline is explicitly Phase 5's scope (upload endpoint,
validation, variant generation, storage abstraction). Building it now
would mean building it twice. The `Photo` entity's shape doesn't
change when Phase 5 lands - it'll populate these same
Thumbnail/Medium/LargePath fields via the real pipeline instead of the
seeder.

**Decision:** EXIF stripped from the *served* image files; the same
data lives in the `Photo` entity instead and is rendered back out
explicitly via the DataLabel component.
**Why:** serving files with embedded GPS/camera metadata leaks more
than intended (anyone can extract it from the raw file) and can't be
selectively hidden per-field. Keeping it in the database means the
site controls exactly what's shown and could redact it entirely later
without re-processing images.

**Decision:** `PhotographyController` is GET-only; no create/update/
delete endpoints yet.
**Why:** write operations need authentication, which doesn't exist
until Phase 6 (Identity, Login, Authorization). An unauthenticated
write endpoint would be a real vulnerability, not just incomplete -
so it simply doesn't exist yet rather than existing insecurely.

**Decision:** Custom Lightbox component (portal-rendered, Escape/
arrow-key navigation, basic focus handling) instead of a UI library.
**Why:** no new dependency for something this contained, and it needed
to match the site's specific visual language (DataLabel EXIF footer)
that a generic library wouldn't provide out of the box.

**Decision:** Lightbox is full-bleed - the photo fills the entire
viewport (against `100% × 100%`), and all chrome (close button,
counter, caption, EXIF, full-resolution link) is an absolutely
positioned overlay with a gradient scrim, not separate header/footer
rows.
**Why:** matches the reference layout the user pointed to (a
commercial gallery script, X3 Photo Gallery / Imagevue - not cloned
directly, but the underlying pattern is standard). Stacked header/
image/footer rows each claim a fixed slice of vertical space and
shrink the photo itself; overlaying the chrome on top of a full-bleed
image gives the photo nearly the entire viewport regardless of
orientation, which is what makes portrait and landscape both feel
immersive without needing separate handling.

**Decision:** Within that full-bleed layout, the photo starts
fit-to-screen (`object-contain`, whole photo visible, letterboxed on
one axis) and clicking it toggles to fill-screen (`object-cover`,
edge-to-edge, cropping overflow on one axis). State resets to fit on
navigating to another photo.
**Why:** preference - gives an immersive full-screen option on demand
without losing the safer default of always seeing the whole photo
first. A separate "view full resolution" new-tab link stays available
for inspecting actual pixel detail, which fit/fill alone don't
provide.
