# Architecture Overview

## Status
Phase 0 — foundation only. No auth, no database, no image upload yet.

## Shape
Modular monolith, not microservices (spec section 4). Two deployables:

- `apps/web` — React + TypeScript SPA (Vite). Presentation and client
  interaction only. Never contains business rules.
- `apps/api` — ASP.NET Core Web API, split into four projects so
  responsibilities can't blur:
  - `Portfolio.Domain` — entities, enums, value objects. No dependencies.
  - `Portfolio.Application` — use cases, orchestration, interfaces that
    Infrastructure implements. Depends only on Domain.
  - `Portfolio.Infrastructure` — EF Core, Identity, filesystem storage,
    image processing. Depends on Domain + Application.
  - `Portfolio.Api` — controllers, middleware, DI wiring. Depends on
    Application + Infrastructure. This is the only project that knows
    about HTTP.

The API is the single source of truth for business rules, validation,
and authorization. The SPA is never trusted (spec principle #2).

## Why this split
It's the cheapest way to keep "business logic embedded in React
components" and "database concerns leaking into controllers" from
happening by accident — the project reference graph makes both
physically impossible to compile, not just a code-review convention.

## Local dev
- SPA dev server: `apps/web`, `npm run dev` → http://localhost:5173,
  proxies `/api/*` to the API.
- API: `apps/api/Portfolio.Api`, `dotnet run` → http://localhost:5199.
- No database yet — Phase 2 introduces EF Core + SQL Server + the
  first migration.

## What's deliberately not here yet
Authentication, EF Core/database, image upload and processing, the
real homepage/design system. See `docs/architecture/decisions.md` for
why each phase is sequenced the way it is, and the spec's own
Phase 0–10 breakdown for the full plan.
