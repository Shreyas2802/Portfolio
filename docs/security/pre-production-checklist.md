# Pre-Production Security Checklist

Walk this before the site goes live at portfolio.vistrut.com. Each
item below is annotated with its actual status in this codebase as of
Phase 9/10 - not a blank template.

- [x] **HTTPS enforced** — Plesk provides free Let's Encrypt SSL for
      the subdomain (Phase 10). `UseHttpsRedirection()` is active in
      every environment except `Testing`.
- [ ] **Secure cookies** — N/A. Auth is JWT-bearer, not cookie-based
      (Phase 6a) - there are no auth cookies to mark Secure/HttpOnly.
- [x] **Admin endpoints protected** — every write endpoint requires
      `[Authorize(Roles = "Admin")]` (Phase 6). Verified in
      `AuthorizationTests` (Phase 9) across every admin route.
- [x] **Default-deny authorization reviewed** — write endpoints are
      allow-listed via `[Authorize]`; nothing is protected by
      forgetting to mark it public. Confirmed by reading every
      controller action during Phase 9.
- [x] **CORS restricted** — exact-origin policy, no `AllowAnyOrigin()`
      (Phase 7). **Action needed before deploy:** add the real
      production frontend origin (`https://portfolio.vistrut.com`) to
      the CORS policy in `Program.cs` - see Phase 10 notes.
- [x] **CSRF protection reviewed** — not applicable by design: no
      cookie auth, so the attack CSRF exploits doesn't apply here
      (Phase 7 decision log has the full reasoning).
- [x] **Rate limiting enabled** — Login/Contact/Upload, per-IP
      (Phase 7). Manually verified working via Postman.
- [x] **Upload limits enabled** — 25MB file size, 30MB request body,
      50-megapixel dimension cap (Phase 5/7/8).
- [x] **File type validation enabled** — validated by actually
      decoding the file as an image, not by trusting extension or
      declared content-type (Phase 5).
- [x] **Path traversal protections tested** — `LocalPhotoStorageService`
      resolves and verifies every path stays inside the uploads root
      before any read/write (Phase 5).
- [ ] **XSS protections tested** — React escapes rendered text by
      default (no `dangerouslySetInnerHTML` anywhere in this codebase),
      and `SafeUrlValidator` blocks `javascript:`-scheme links
      (Phase 7, unit-tested in Phase 9). Not independently penetration-
      tested beyond that - reasonable for this site's risk level, but
      worth knowing the limit of what's been verified.
- [ ] **SQL injection protections tested** — EF Core's LINQ query
      building parameterizes everything by construction; there is no
      raw SQL anywhere in the codebase. Not separately fuzz-tested,
      since there's no raw-SQL surface for that class of test to
      target in the first place.
- [x] **Security headers configured** — CSP, X-Content-Type-Options,
      Referrer-Policy, Permissions-Policy, X-Frame-Options, HSTS
      outside dev (Phase 7). **Caveat:** these currently only apply to
      API responses, not the SPA's own HTML - see Phase 10 for how
      deployment addresses this.
- [ ] **Secrets removed from repository** — `Jwt:Key`, `AdminUser:*`,
      and (previously) `AdminUploadKey` were always kept in .NET User
      Secrets, never committed. **Action needed before deploy:**
      confirm the production connection string and JWT key are set
      via Plesk environment variables, not a committed
      `appsettings.Production.json` - see Phase 10.
- [x] **Production error details disabled** — `UseExceptionHandler()`
      (ProblemDetails, no stack traces) outside Development; confirmed
      in Program.cs since Phase 2.
- [ ] **Logs reviewed for secrets** — failed-login logging (Phase 7)
      explicitly logs username only, never password. No other
      security-relevant logging exists yet to review.
- [x] **Dependency vulnerabilities checked** — `npm audit`: 0
      vulnerabilities. `dotnet list package --vulnerable
      --include-transitive`: 0 vulnerabilities across all four backend
      projects (checked Phase 7, re-check before deploy since this can
      change over time).
- [ ] **Database permissions minimized** — the local dev connection
      string uses Windows-integrated auth (no separate DB user/
      password to scope). **Action needed before deploy:** the Plesk
      production SQL Server login should be scoped to only the
      `PortfolioDb` database, not a sysadmin/server-wide account - see
      Phase 10.
- [ ] **Backup tested** — see Phase 10's backup section. Not yet
      executed against the real production database (doesn't exist
      until deployment).
- [ ] **Restore tested** — same as above; a backup that's never been
      restored isn't a verified backup. Do this once after the first
      real production backup exists.

**Unchecked items above are exactly the ones Phase 10 (Deployment)
needs to close out** - they're either genuinely deployment-specific
(can't be true until a production environment exists) or explicitly
flagged as "not yet done" rather than silently assumed complete.
