# Portfolio Platform

Personal portfolio + photography platform. See
[`docs/architecture/overview.md`](docs/architecture/overview.md) for
the architecture and the original spec for the full plan.

**Status:** Phase 0 — foundation only.

## Prerequisites
- Node.js 20+ and npm
- .NET SDK 10 (or whatever the current LTS is when you set this up —
  check with `dotnet --version`)
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

With both running, open http://localhost:5173 — the page should show
a green "API status: ok" line. If it's red, the API isn't running or
isn't reachable on port 5199 (check `apps/web/vite.config.ts` and
`apps/api/Portfolio.Api/Properties/launchSettings.json` if you change
ports).

## Verifying Phase 0
```bash
# frontend
cd apps/web && npm run lint && npm run build

# backend
cd apps/api && dotnet build
```
Both should complete with no errors before moving to Phase 1.

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
