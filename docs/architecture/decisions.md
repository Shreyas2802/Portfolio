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

---

## Phase 5 — Image Processing (upload endpoint, validation, EXIF, variants)
**Decision:** SkiaSharp (MIT) for decode/resize/encode, MetadataExtractor
(Apache 2.0) for EXIF reading - not ImageSharp.
**Why:** the spec explicitly calls this out - ImageSharp's recent
versions require a commercial license above a revenue threshold, and
the spec says not to make it a mandatory dependency. SkiaSharp and
MetadataExtractor both stay genuinely free regardless of the site's
future use, matching the "free/open-source stack only" constraint
with no ambiguity.

**Decision:** File validity is checked by actually decoding the bytes
with SkiaSharp, never by trusting the declared content-type or file
extension.
**Why:** a client can label any file "image/jpeg" - the only real
check is whether it decodes as one. This also means corrupt or
truncated uploads are rejected with a clear error instead of being
silently stored as broken images.

**Decision:** The upload endpoint is gated by a temporary shared-secret
header (`X-Admin-Key`), checked against an `AdminUploadKey` value in
.NET User Secrets (never committed) - not `[Authorize]`, since Identity
doesn't exist until Phase 6.
**Why:** Phase 5 explicitly comes before Phase 6 in the spec's own
plan, which means real auth genuinely isn't available yet - but
leaving a file-upload endpoint completely open in that gap is a real
risk, not just an incompleteness. The check fails closed: if the key
isn't configured at all, no request can ever match, so upload is
unreachable by default rather than silently open. This is explicitly
temporary - replace with `[Authorize(Roles = "Admin")]` and delete the
`AdminUploadKey` configuration entirely once Phase 6 lands.

**Decision:** On any failure after variant files have been written
(e.g. the database save fails), all already-saved variants for that
upload are deleted before returning an error.
**Why:** spec Phase 5 explicitly calls for cleanup/error handling - a
failed upload should never leave orphaned files with no corresponding
database row. Cleanup failures are logged but don't mask the original
error returned to the caller.

**Decision:** `IPhotoStorageService` abstracts "save a variant, get a
URL back" / "delete a variant" - `LocalPhotoStorageService` is the
only implementation today (writes to `wwwroot/uploads`), with path
traversal explicitly blocked by resolving and verifying the final path
stays inside the uploads root.
**Why:** matches the spec's "storage abstraction" requirement -
swapping to cloud storage later means writing one new class, not
touching the upload service, the controller, or any calling code.

**Fix:** build failed with `CS0234`/`CS0246` on `IHostEnvironment` in
`Portfolio.Infrastructure`. Cause: that project is a plain class
library (`Microsoft.NET.Sdk`), not a web project - `IHostEnvironment`,
and typically `ILogger<T>`, are part of the ASP.NET Core shared
framework that `Portfolio.Api` gets for free via `Sdk.Web`, but a
plain library needs them as explicit NuGet packages. Fixed by adding
`Microsoft.Extensions.Hosting.Abstractions` and
`Microsoft.Extensions.Logging.Abstractions` package references
directly to `Portfolio.Infrastructure.csproj`.

---

## Phase 6a — Admin auth foundation (Identity, Login, Authorization)
**Decision:** `ApplicationUser : IdentityUser` lives in
`Portfolio.Infrastructure/Identity/`, not `Portfolio.Domain` -
breaking the "every entity lives in Domain" pattern used everywhere
else in this codebase.
**Why:** `IdentityUser` is inherently framework-coupled (password
hashing, EF mapping, lockout state) - putting it in Domain, which is
kept deliberately dependency-free, would mean either compromising that
boundary or wrapping Identity in an adapter for no real benefit on a
single-owner site. The honest inconsistency (documented here) beats
a technically-pure but pointless abstraction layer.

**Decision:** JWT bearer tokens, not cookie-based auth.
**Why:** matches JWT experience already on the site owner's resume
(AuthServer project, Brahmanda Tatva), and keeps the API stateless -
no server-side session store, consistent with this being a pure JSON
API behind a separate SPA rather than a server-rendered app. Used
`AddIdentityCore` (not `AddIdentity`) specifically because it skips
the cookie/UI scheme setup that a pure API doesn't need.

**Decision:** No public registration endpoint. The single admin
account is created from `AdminUser:Username`/`AdminUser:Password`
configuration (User Secrets, never committed) via `IdentitySeeder`,
which runs on every startup but only acts once - idempotent, and a
no-op entirely if those secrets aren't set.
**Why:** this is a single-owner personal site, not multi-tenant SaaS -
a registration flow would be attack surface for a problem that doesn't
exist here. `Role` still exists properly (an "Admin" `IdentityRole` is
seeded too) so `[Authorize(Roles = "Admin")]` is real role-based
authorization, not just "any logged-in user."

**Decision:** `PhotographyController.Upload` now uses
`[Authorize(Roles = "Admin")]`, replacing the temporary `X-Admin-Key`
header check from Phase 5 entirely - the `AdminUploadKey`
configuration value is no longer read anywhere.
**Why:** this was always the planned replacement, flagged explicitly
in the Phase 5 entry above. Real role-based authorization is strictly
stronger than a shared-secret header, and removing the old check
avoids leaving two competing gates on the same endpoint.

**Decision:** Login always returns the same generic "Invalid username
or password" message, whether the username doesn't exist or the
password is wrong.
**Why:** returning a different message for "no such user" vs "wrong
password" lets an attacker enumerate valid usernames one guess at a
time - a well-known and easily avoided information leak.

**Note:** this covers Identity/Login/Authorization only. Dashboard,
Photo/Album/Category/Tag/Project management (the admin UI itself)
follow in the next part of Phase 6 - there is still no way to log in
or manage content through the browser yet, only via direct API calls.

---

## Phase 6b — Admin UI (login, dashboard, photo management)
**Decision:** JWT stored in `localStorage` (via a small `AuthContext`
+ `authStorage.ts`), attached as `Authorization: Bearer <token>` on
every authenticated request - not an HttpOnly cookie.
**Why:** simplest option that works cleanly with a SPA making direct
fetch calls, and matches the JWT-bearer choice already made on the
backend. The tradeoff (XSS could theoretically read `localStorage`)
is accepted here because there is exactly one admin user securing
their own personal site's content, not a multi-user app handling
other people's sensitive data - a cookie-based approach would be the
right call at a different risk profile.

**Decision:** `ProtectedRoute` (client-side redirect to `/admin/login`
if not authenticated) is explicitly documented as a UX convenience,
not the real security boundary - that's the server's
`[Authorize(Roles = "Admin")]`.
**Why:** a client-side check can always be bypassed by editing
`localStorage`; the point of stating this plainly in the code is so a
future change never mistakes the frontend guard for actual protection
and skips adding the matching backend check.

**Decision:** Added `GET /api/photography/photos` (admin-only, returns
every photo including unpublished ones) rather than reusing the public
`featured`/`albums/{slug}` endpoints for the admin photo list.
**Why:** the public endpoints intentionally only return published
photos and are scoped to one album at a time - the admin list needs
everything, across all albums, regardless of publish state.

**Decision:** Deleting a photo removes the database row and best-
effort deletes its three variant files, logging (not failing) if a
file is already missing.
**Why:** a missing file on disk shouldn't block removing the database
record, which is the part that actually matters to the admin - an
already-gone file isn't a real error worth surfacing.

---

## Phase 6c — Album, Category, and Tag management
**Decision:** Album `Slug` is never regenerated on an edit, even when
the title changes.
**Why:** once an album is published, its URL should stay stable - a
title tweak shouldn't silently break a link someone already shared or
bookmarked. Only the initial create generates a slug.

**Decision:** Deleting an album deletes each contained photo's variant
files from disk first, then removes the album (which cascade-deletes
the photo rows).
**Why:** without this, cascade-delete would remove the database rows
but leave the actual image files orphaned on disk with nothing
pointing at them - the same orphan-file problem the Phase 5 upload
rollback logic exists to prevent, just triggered from a different
direction.

**Decision:** Deleting a category does not delete its albums - the
existing `DeleteBehavior.SetNull` on `Album.CategoryId` (from Phase 4)
means albums simply become uncategorized.
**Why:** a category is organizational metadata, not a container - an
album's photos are the real content and shouldn't be threatened by
deleting an unrelated grouping label.

**Decision:** Tag management (create/list/delete) is built, but
assigning tags to individual photos is not wired into the upload form
yet.
**Why:** honest scope-matching - today's admin only supports upload
and delete for photos, not edit, so there's no natural place yet for
a tag picker. This satisfies the spec's "tag management" bullet at the
entity level; per-photo tagging is a natural follow-up once a photo-
edit screen exists.

**Decision:** Found and fixed a bug while building this: the photo
upload form's album dropdown was calling the *public* albums endpoint,
which only returns published albums - so a newly created (or
deliberately unpublished) album never appeared as an upload target.
Switched it to the new admin "all albums" endpoint.
**Why:** this is exactly the "why can I only see one album" issue
flagged before this phase started - there was always meant to be a
way to create more, the admin UI just wasn't querying the right
endpoint to see them once created.

**Fix:** the upload form asked for Album but not Tags, and uploading a
photo into a second album never showed up on the public `/work` page.
Two separate causes:
- Tags simply weren't wired into the upload form or the upload
  service at all - added a tag multi-select to the form, `TagIds` on
  `UploadPhotoRequest`, and assignment to `Photo.Tags` before saving.
  Category was intentionally left off this form - Category groups
  Albums, not individual Photos, so it's set at the album level (see
  Phase 4), not per-upload.
- `WorkPage` only ever fetched `albums?.[0]?.slug` - the very first
  album returned, full stop. With only one album ever existing before
  this phase, that bug was invisible; the moment a second album
  existed, its photos became permanently unreachable from the public
  gallery regardless of publish status. Rebuilt the page to list every
  published album as tabs and load whichever one is selected.

**Fix:** portrait photos uploaded through the real pipeline (as
opposed to the Phase 4 seed data, which used a different tool) were
saved and displayed sideways. Cause: cameras commonly store sensor
pixel data in a fixed landscape layout and rely on the EXIF
Orientation tag to tell viewers how to rotate/flip it for correct
display - SkiaSharp's `SKBitmap.Decode` does not apply this
automatically, it just returns the raw pixels as stored. The seed
data happened to go through a tool (Python/Pillow's
`ImageOps.exif_transpose`) that does apply it, which is why that path
looked correct while the real upload pipeline didn't. Fixed by reading
the Orientation tag via MetadataExtractor and physically
rotating/flipping the decoded bitmap (`ApplyExifOrientation`, covering
all 8 EXIF orientation values) before generating variants or storing
`Photo.Width`/`Height` - the corrected dimensions matter too, since
the frontend's `aspect-ratio` CSS for each photo tile depends on them
being right. The saved variant files need no orientation tag
afterward, consistent with the existing choice to strip EXIF from
served files entirely.

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

**Decision:** When the browser is in F11 (native OS-level) fullscreen,
the bottom scrim - caption, EXIF, fill hint, full-resolution link - is
hidden entirely; only the top counter/close controls remain.
**Why:** the point of pressing F11 is an unobstructed view of the
photo, so keeping the same chrome up defeats that. F11 fullscreen
doesn't fire the Fullscreen API's `fullscreenchange` event (that only
covers `element.requestFullscreen()` calls) - detected instead by
comparing `window.innerWidth/innerHeight` to `window.screen.width/
height` on resize, the standard technique since F11 hides browser
chrome without involving the Fullscreen API at all.
