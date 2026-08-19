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
