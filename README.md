# Portfolio Platform

Personal portfolio + photography platform. See
[`docs/architecture/overview.md`](docs/architecture/overview.md) for
the architecture and the original spec for the full plan.

**Status:** Phase 0 — foundation only.

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
