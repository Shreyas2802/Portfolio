# Build log

Personal portfolio + photography platform. See
[`docs/architecture/overview.md`](docs/architecture/overview.md) for
the architecture and the original spec for the full plan.

Phase-by-phase notes kept from the original build. For the current overview, see the main README.

## Prerequisites
- Node.js 20+ and npm
- .NET SDK 9 (matches the `net9.0` target in the `.csproj` files —
  check with `dotnet --version`; bump both together if you upgrade)
- Git

## Running locally

### Frontend
```bash
cd apps/web
npm install
npm run dev        # http://localhost:5173
```

### Backend
```bash
cd apps/api
dotnet restore
dotnet build
dotnet run --project Portfolio.Api   # http://localhost:5199
```

### Database (first time, or after pulling new migrations)
Requires the `dotnet-ef` tool (`dotnet tool install --global dotnet-ef`
if you don't have it) and SQL Server LocalDB (ships with Visual
Studio).
```bash
cd apps/api
dotnet ef database update --project Portfolio.Infrastructure --startup-project Portfolio.Api
```
This creates the `PortfolioDb` database in LocalDB and applies all
migrations. After this, `GET /api/health` should report
`"database": "connected"`.

To add a new migration after changing entities:
```bash
dotnet ef migrations add <DescriptiveName> --project Portfolio.Infrastructure --startup-project Portfolio.Api
```
Phase 3 adds a `ContactMessage` entity — after pulling these changes,
add and apply a new migration for it (see "Add a new migration" above,
then `database update` again) before the contact form will work.

Phase 4 adds `Category`, `Album`, and `Photo` entities — same process:
add a migration (e.g. `AddPhotographyModel`), then `database update`.
On first run in Development, the API automatically seeds the gallery
with real photos (see `wwwroot/uploads/photos/` and `DbSeeder.cs`) —
no manual data entry needed to see the gallery working.

Phase 5 adds the real upload pipeline (no entity changes, no new
migration needed). Before it will work, set a temporary admin key —
this gates the upload endpoint until real Identity/auth lands in
Phase 6:
```bash
cd apps/api/Portfolio.Api
dotnet user-secrets set "AdminUploadKey" "<pick-any-string>"
```
This is stored outside the repo (never committed) via .NET's User
Secrets, already wired up via the `UserSecretsId` in
`Portfolio.Api.csproj`. To test the upload endpoint, send a
`multipart/form-data` POST to `/api/photography/upload` with fields
`file`, `title`, `caption` (optional), `albumId`, and header
`X-Admin-Key: <the value you set>` — Postman or curl both work fine.
Without the header (or with the wrong value), the endpoint returns
401 — there is no "upload UI" yet; that's Phase 6's admin dashboard.

Phase 6 (auth foundation) replaces `AdminUploadKey` with real Identity
and JWT auth — the upload endpoint no longer checks `X-Admin-Key` at
all. New entities (ASP.NET Core Identity's own tables) need a
migration, and three secrets need to be set before login works:
```bash
cd apps/api/Portfolio.Api
dotnet user-secrets set "Jwt:Key" "<a long random string, 32+ chars>"
dotnet user-secrets set "AdminUser:Username" "<pick a username>"
dotnet user-secrets set "AdminUser:Password" "<pick a password, 12+ chars>"
```
Then from `apps/api`:
```bash
dotnet ef migrations add AddIdentity --project Portfolio.Infrastructure --startup-project Portfolio.Api
dotnet ef database update --project Portfolio.Infrastructure --startup-project Portfolio.Api
```
On next run, the admin user is created automatically from those
secrets. To log in, `POST /api/auth/login` with JSON body
`{ "username": "...", "password": "..." }` — the response includes a
`token`. Pass it as `Authorization: Bearer <token>` on the upload
endpoint instead of the old `X-Admin-Key` header.

There is still no admin *UI* yet — login and CRUD screens are the
next part of Phase 6. Test via Postman/curl for now.

Phase 6b adds the admin UI itself (no new migration needed): visit
`/admin/login` in the browser, sign in with the `AdminUser` credentials
you set above, and you'll land on a dashboard with photo/album/project/
unread-message counts. `/admin/photos` has a real upload form (file,
title, caption, album picker) and a grid of every photo with a delete
button on hover. This finally replaces Postman for day-to-day use —
Postman/curl are still useful for testing the API directly, but aren't
required anymore.

Phase 6c adds Album, Category, and Tag management (no new migration
needed - same entities as Phase 6a). `/admin/albums` now lets you
create new albums, edit existing ones, and delete them (which also
removes their photos and files). `/admin/taxonomy` manages Categories
and Tags. Note: assigning tags to individual photos isn't built yet -
that needs a photo-edit screen, which doesn't exist yet either (today
you can upload and delete photos, not edit their details after the
fact).

**Fix (same phase):** the upload form now includes a tag picker (Tags
apply to individual photos; Category applies to the album as a whole,
set when creating/editing the album, not per-photo). Also fixed: the
public `/work` gallery only ever showed the *first* album - it now
shows tabs for every published album. If a photo still isn't showing
up after this fix, check that its album's "Published" checkbox is
checked in `/admin/albums`.

**Fix (portrait photos sideways):** portrait photos uploaded through
the real pipeline were being saved rotated/sideways - the EXIF
Orientation tag wasn't being applied to the actual pixels. This is
now fixed for all future uploads. Any photo already uploaded *before*
this fix and displaying sideways needs to be deleted and re-uploaded
from `/admin/photos` - existing database rows/files aren't
automatically corrected.

Phase 6d (Project management, the final piece of Phase 6) changes the
`Project` entity's shape - add and apply a new migration:
```bash
dotnet ef migrations add UpdateProjectFields --project Portfolio.Infrastructure --startup-project Portfolio.Api
dotnet ef database update --project Portfolio.Infrastructure --startup-project Portfolio.Api
```
On next run, the five projects that used to live as static frontend
data are seeded into the database automatically (once, like the photo
seed). The public `/projects` page and project detail pages now come
from the API instead of a hardcoded file. `/admin/projects` has full
create/edit/delete, including a "Featured" toggle that controls
whether a project shows as a large case-study card or a smaller
"other work" entry.

Phase 7 (Hardening) adds no new entities - no migration needed. Verify
it after pulling:
```bash
# Rate limiting: try logging in with a wrong password 6 times quickly -
# the 6th attempt should return 429 Too Many Requests instead of 401.

# Security headers: check any API response
curl -I http://localhost:5199/api/photography/albums
# Should include X-Content-Type-Options, Referrer-Policy,
# Permissions-Policy, X-Frame-Options, and Content-Security-Policy.

# Dependency audit:
cd apps/api
dotnet list package --vulnerable --include-transitive
```
Note: the security headers above apply to the API's own responses.
They do **not** currently apply to the React app's HTML page itself,
since that's served by Vite's dev server (a different process/port)
in local development - see the architecture decision log for why, and
what Phase 10 needs to do about it.

Phase 8 (Performance) adds two new database indexes on existing
tables and changes the shape of a couple of endpoints - no new
entities, but you do need a migration for the indexes:
```bash
dotnet ef migrations add AddPerformanceIndexes --project Portfolio.Infrastructure --startup-project Portfolio.Api
dotnet ef database update --project Portfolio.Infrastructure --startup-project Portfolio.Api
```
The public gallery (`/work`) now loads photos a page at a time with a
"Load more" button instead of all at once; `/admin/photos` gained
Previous/Next page controls. The admin panel's JavaScript is now a
separate chunk that only downloads when someone actually visits
`/admin/*` - check this by opening the Network tab on the public site
and confirming no `AdminLayout`/`PhotosAdminPage`/etc. chunks load
until you navigate there.

## Testing (Phase 9)

**Unit tests** (`apps/api/Portfolio.Tests.Unit`) - pure logic, no
database or network needed:
```bash
cd apps/api
dotnet test Portfolio.Tests.Unit
```

**Integration tests** (`tests/api-integration/Portfolio.Tests.Integration`) -
boots the real app against an in-memory SQLite database, no SQL
Server/LocalDB required:
```bash
dotnet test ../tests/api-integration/Portfolio.Tests.Integration
```
Or from the solution root (`apps/api`), `dotnet test` runs both test
projects together alongside a full solution build.

**End-to-end tests** (`tests/web-e2e`, Playwright) - needs both dev
servers actually running first. See `tests/web-e2e/README.md` for
full setup; short version:
```bash
cd tests/web-e2e
npm install
npx playwright install chromium
npm test
```
Admin-flow E2E tests are skipped unless you pass
`E2E_ADMIN_USERNAME`/`E2E_ADMIN_PASSWORD` matching your local
`AdminUser` secrets.

**Security checklist:** see
[`docs/security/pre-production-checklist.md`](docs/security/pre-production-checklist.md)
before deploying - it's filled in with this codebase's actual current
status per item, not a blank template.

## Deployment (Phase 10)

Full runbook: [`docs/deployment/plesk-deployment-guide.md`](docs/deployment/plesk-deployment-guide.md).

Quick version:
```powershell
.\deploy\build-and-publish.ps1
```
builds the frontend, copies it into the API's `wwwroot` (never
touching `wwwroot\uploads`), and publishes the backend to `.\publish` -
that folder is what gets uploaded to Plesk. The guide covers
environment variables/secrets, the production database, keeping
uploaded photos safe across redeploys, HTTPS, monitoring, backups, and
CI/CD in full.

CI (build + test on every push) is already configured in
`.github/workflows/ci.yml` and needs no setup - it just runs on GitHub.

With both running, open http://localhost:5173 — the page should show
a green "API status: ok" line. If it's red, the API isn't running or
isn't reachable on port 5199 (check `apps/web/vite.config.ts` and
`apps/api/Portfolio.Api/Properties/launchSettings.json` if you change
ports).

## Verifying the setup
```bash
# frontend
cd apps/web && npm run lint && npm run build

# backend
cd apps/api && dotnet build

# database
cd apps/api && dotnet ef database update --project Portfolio.Infrastructure --startup-project Portfolio.Api
```
All three should complete with no errors. Then `dotnet run --project
Portfolio.Api` and check `GET http://localhost:5199/api/health` shows
`"database": "connected"`.

## Project layout
```text
apps/
  web/    React + TypeScript + Vite + Tailwind
  api/    ASP.NET Core (Domain / Application / Infrastructure / Api)
docs/
  architecture/   architecture overview + decisions log
tests/
  web-e2e/            Playwright (added in Phase 9)
  api-integration/    ASP.NET Core integration tests (added in Phase 9)
```
