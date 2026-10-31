# End-to-end tests (Playwright)

These run against **real, already-running** dev servers — they don't
start their own. Before running:

```bash
# Terminal 1
cd apps/api && dotnet run --project Portfolio.Api

# Terminal 2
cd apps/web && npm run dev
```

## Setup

```bash
cd tests/web-e2e
npm install
npx playwright install chromium
```

## Running

```bash
npm test                # headless
npm run test:headed     # see the browser
npm run test:ui         # Playwright's interactive UI mode
```

Public-site tests (`public-site.spec.ts`) run with no extra setup.

Admin tests (`admin.spec.ts`) need the admin credentials you already
set via `dotnet user-secrets` (see the main README) passed as
environment variables, since nothing here should ever hardcode real
credentials:

```bash
E2E_ADMIN_USERNAME=<your username> E2E_ADMIN_PASSWORD=<your password> npm test
```

Without those set, the admin tests are automatically skipped rather
than failing.

The upload test cleans up after itself (deletes the photo it created),
but if a run is interrupted partway, check `/admin/photos` for a
leftover `E2E Test Photo ...` entry and delete it manually.
