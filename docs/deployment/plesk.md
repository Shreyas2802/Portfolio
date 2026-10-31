# Production deployment (Plesk)

This project is packaged as one ASP.NET Core application: the API serves the built React SPA from `wwwroot`, while photo originals and generated variants live outside the release directory. This produces one HTTPS origin and avoids production CORS.

## Host prerequisites

Confirm these items in Plesk before attempting a release. The repository intentionally does not assume Windows or Linux hosting.

- ASP.NET Core Runtime 9.0 is available, or the app is retargeted to the host's supported LTS runtime.
- The subscription can run an ASP.NET Core application behind Plesk's reverse proxy.
- SQL Server is available and the application database/user can be created.
- A writable, non-web-root directory can be reserved for `PhotoStorage:RootPath`.
- A managed TLS certificate can be issued for the production domain and its `www` variant, if used.

## Production configuration

Set these values in Plesk's secure environment-variable/configuration facility; do not commit them.

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<least-privilege SQL Server connection string>
Jwt__Key=<at least 32 random bytes, base64 or high-entropy text>
Jwt__Issuer=PortfolioApi
Jwt__Audience=PortfolioClient
AdminUser__Username=<admin username>
AdminUser__Password=<unique 12+ character password>
PhotoStorage__RootPath=<absolute persistent path outside the release directory>
Database__ApplyMigrationsOnStartup=true
Seed__InitialContent=true
ReverseProxy__UseForwardedHeaders=true
```

Use a SQL login dedicated to this application. It needs only the database permissions required by the app and migrations; it must not be a server administrator. After the first successful release, set `Database__ApplyMigrationsOnStartup=false` and `Seed__InitialContent=false`. Run migrations as an explicit deployment step when schema changes are introduced.

## Build and publish

From the repository root, run:

```powershell
./scripts/prepare-release.ps1
```

Upload the contents of `artifacts/release` to the Plesk ASP.NET Core application's physical path. Configure its startup assembly as `Portfolio.Api.dll` and point its application URL/proxy to the public domain.

Before the first startup, copy the **contents** of `artifacts/release/initial-media` into `PhotoStorage__RootPath`. Do not copy `initial-media` into the web root. This provides the initial public gallery while keeping media outside the release directory.

Enable HTTPS redirection in Plesk and force HTTP to HTTPS. Confirm the certificate covers every configured hostname. The application adds HSTS in Production after the TLS path is working.

## Release verification

- `https://<domain>/api/health` returns HTTP 200 with `database: connected`.
- The homepage, `/work`, a project page, and a deep-link refresh all load.
- Log in as admin, upload one non-sensitive image, and verify it persists after an application restart.
- Confirm an anonymous request to `/api/photography/photos` receives 401.
- Check response headers for CSP, HSTS, `X-Content-Type-Options`, and `X-Frame-Options`.
- Review application logs for startup/migration errors and ensure no secret appears in them.

## CI/CD boundary

The included GitHub workflow verifies every pull request and `main` push. It intentionally does not deploy: Plesk transport differs by host (SSH, Git, FTP, or provider integration). After the host is confirmed, add a protected production environment and a deploy job using the host's supported transport. That job must run only after both `web` and `api` jobs succeed, and its host credentials must be GitHub environment secrets.
