# Personal Portfolio + Photography Platform
## Codex Master Specification

> **Project goal:** Build a production-quality personal website that combines a software-development portfolio, a high-end photography portfolio, and a private admin CMS for managing content and photographs.
>
> **Primary learning goal:** The project must give the developer substantial hands-on experience with React + TypeScript while also demonstrating professional ASP.NET Core, REST API, SQL Server, authentication, image processing, performance, security, testing, and deployment practices.
>
> **Cost constraint:** Prefer free and open-source software and services. Do not introduce paid image/CDN/cloud services unless explicitly approved later. Image processing must run locally/on the hosting server using free/open-source libraries.

---

# 1. Product Vision

The site is a personal digital identity with two major professional pillars:

1. **Photography**
   - Visually dominant.
   - High-resolution photography.
   - Albums, categories, tags, featured images.
   - Full-screen image viewer.
   - Responsive image delivery.
   - EXIF metadata where appropriate.
   - Future support for private/client galleries.

2. **Software Development**
   - About.
   - Skills/technology stack.
   - Projects.
   - Project case studies.
   - GitHub/live-demo links.
   - Experience/certifications/resume.
   - Architecture and technical explanations where useful.

The public website should feel premium, minimal, editorial, fast, and image-first rather than like a generic developer template.

The admin application should be functional, clean, secure, and optimized for productivity.

---

# 2. Non-Negotiable Requirements

## UX
- Excellent on mobile, tablet, laptop, desktop, ultrawide monitors, and TV-sized screens.
- Responsive from approximately 320px wide through large 4K/TV displays.
- Touch-friendly.
- Keyboard accessible.
- Respect prefers-reduced-motion.
- No layout shifts caused by images.
- Fast first load.
- Premium visual hierarchy.
- Do not copy Apple/Tesla branding, layouts, assets, or proprietary design. Use them only as inspiration for restraint, typography, whitespace, product-like presentation, and motion.

## Engineering
- Clear separation of concerns.
- Strong typing.
- No unnecessary global state.
- No business logic embedded in React presentation components.
- No database blobs for images.
- No secrets in source control.
- No hard-coded production credentials.
- No insecure "temporary" authentication that becomes permanent.
- All important operations validated server-side.
- Public endpoints must not expose admin functionality.
- Admin endpoints must require authorization.
- Every significant feature must have tests.
- Every production dependency must have a clear purpose.

## Cost
- Prefer free/open-source libraries.
- Use local/server-side image processing.
- Use filesystem storage initially.
- No mandatory paid cloud storage.
- No mandatory paid CDN.
- No mandatory paid image optimization service.
- Architecture must allow storage/CDN migration later without redesigning the domain model.

---

# 3. Recommended Technology Stack

## Frontend
- React
- TypeScript
- Vite
- Tailwind CSS
- React Router
- TanStack Query for server-state management
- Zod for client-side boundary validation where useful

Do not add Redux unless a real requirement emerges.

## Backend
- ASP.NET Core Web API
- Current supported LTS .NET version available at implementation time
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Policy/role-based authorization

## Image Processing
Use free/open-source libraries.

Preferred initial approach:
- SkiaSharp for image decoding/resizing/encoding.
- MetadataExtractor for EXIF/metadata extraction.

Do not make ImageSharp a mandatory dependency. Re-evaluate image libraries against their current licenses before adding them.

## Testing
Frontend:
- Vitest
- React Testing Library
- Playwright for end-to-end tests

Backend:
- xUnit
- ASP.NET Core integration testing
- EF Core test strategy appropriate to the production SQL Server behavior

## Tooling
- Git
- GitHub
- ESLint
- Prettier
- EditorConfig
- .NET format/analyzers
- Nullable reference types enabled
- Treat warnings seriously; avoid suppressing warnings without justification

---

# 4. High-Level Architecture

Use a modular monolith.

Do NOT start with microservices.

```text
                         INTERNET
                            |
                         HTTPS
                            |
                    Reverse Proxy / Plesk
                            |
                +-----------+-----------+
                |                       |
                v                       v
          React SPA                 ASP.NET Core API
                                      |
                  +-------------------+-------------------+
                  |                   |                   |
                  v                   v                   v
             Application          Infrastructure       Identity
               Layer                  Layer
                  |                   |                   |
                  +-------------------+-------------------+
                                      |
                         +------------+------------+
                         |                         |
                         v                         v
                    SQL Server              File Storage
                                              /uploads
                                                 |
                                                 v
                                      Generated image variants
```

The React application is responsible for presentation and client interaction.

The API is the authoritative source for business rules, authorization, validation, metadata, uploads, and content management.

SQL Server stores structured metadata.

The filesystem stores image files.

---

# 5. Repository Structure

Use a monorepo:

```text
portfolio-platform/
|
+-- apps/
|   +-- web/                         # React + TypeScript
|   +-- api/                         # ASP.NET Core API
|
+-- tests/
|   +-- web-e2e/
|   +-- api-integration/
|
+-- docs/
|   +-- architecture/
|   +-- api/
|   +-- security/
|   +-- deployment/
|
+-- scripts/
|
+-- .editorconfig
+-- .gitignore
+-- README.md
```

Backend structure:

```text
api/
+-- Portfolio.Api/
|   +-- Controllers/
|   +-- Middleware/
|   +-- Configuration/
|   +-- Program.cs
|
+-- Portfolio.Application/
|   +-- Photos/
|   +-- Albums/
|   +-- Categories/
|   +-- Projects/
|   +-- Contact/
|   +-- Identity/
|   +-- Common/
|
+-- Portfolio.Domain/
|   +-- Entities/
|   +-- Enums/
|   +-- ValueObjects/
|   +-- Interfaces/
|
+-- Portfolio.Infrastructure/
    +-- Persistence/
    +-- Identity/
    +-- Storage/
    +-- Images/
    +-- Metadata/
```

The exact project split may be adjusted if implementation proves a simpler structure is better, but domain/application/infrastructure responsibilities must remain clearly separated.

---

# 6. Domain Model

Initial entities:

```text
ApplicationUser
Role

Photo
PhotoVariant

Album
Category
Tag
PhotoTag

Project
ProjectImage

ContactMessage

SiteSettings
```

## Photo

Suggested fields:

```text
Id
Title
Slug
Description
OriginalFileName
StorageKey
MimeType
Width
Height
FileSizeBytes
DateTakenUtc
LocationName
CameraMake
CameraModel
LensModel
FocalLength
Aperture
ShutterSpeed
Iso
AltText
IsPublished
IsFeatured
SortOrder
CreatedAtUtc
UpdatedAtUtc
```

Do not store unnecessary EXIF data. Store only useful metadata.

## PhotoVariant

```text
Id
PhotoId
VariantType
StorageKey
Width
Height
FileSizeBytes
MimeType
CreatedAtUtc
```

Variant types can include:

```text
Thumbnail
Small
Medium
Large
```

Original is stored separately and should never be served for normal gallery rendering unless explicitly requested.

## Album

```text
Id
Name
Slug
Description
CoverPhotoId
IsPublished
SortOrder
CreatedAtUtc
UpdatedAtUtc
```

## Category

Examples:

```text
Automotive
Travel
Nature
Architecture
Street
Portraits
Events
```

## Tags

Many-to-many between Photo and Tag.

## Project

```text
Id
Title
Slug
ShortDescription
Description
Technologies
GithubUrl
LiveUrl
CoverImage
IsPublished
SortOrder
CreatedAtUtc
UpdatedAtUtc
```

---

# 7. Database Rules

- Use EF Core migrations.
- Use UTC timestamps.
- Add indexes for slugs and frequently queried fields.
- Enforce unique slugs.
- Enforce foreign keys.
- Use appropriate cascade/delete behavior.
- Never construct SQL using string concatenation.
- Use EF Core LINQ or parameterized SQL where justified.
- Avoid N+1 queries.
- Project only required columns for public list endpoints.
- Paginate galleries.
- Do not return full entity graphs unnecessarily.

Potential indexes:

```text
Photo.Slug
Photo.IsPublished
Photo.IsFeatured
Photo.CreatedAtUtc
Photo.AlbumId
Photo.CategoryId

Album.Slug
Category.Slug
Project.Slug
```

---

# 8. Image Storage Architecture

Do NOT store image binaries inside SQL Server.

Use a storage abstraction:

```text
IFileStorage
    |
    +-- LocalFileStorage
```

Future:

```text
IFileStorage
    |
    +-- LocalFileStorage
    +-- S3CompatibleStorage
    +-- CloudStorage
```

The rest of the application must depend on the abstraction, not directly on filesystem APIs.

Example conceptual storage:

```text
/storage/
    originals/
        yyyy/
            mm/
                photo-id/
                    original.jpg

    variants/
        yyyy/
            mm/
                photo-id/
                    thumbnail.webp
                    small.webp
                    medium.webp
                    large.webp
```

Do not use user-supplied filenames directly as filesystem paths.

Generate internal storage keys.

Prevent path traversal.

---

# 9. Image Upload Pipeline

Upload flow:

```text
Client
  |
  v
Authenticate
  |
  v
Authorize Admin
  |
  v
Validate request
  |
  v
Validate file size/type
  |
  v
Store temporary upload
  |
  v
Decode image safely
  |
  v
Extract selected metadata
  |
  v
Validate dimensions
  |
  v
Generate variants
  |
  +--> Thumbnail
  +--> Small
  +--> Medium
  +--> Large
  |
  v
Write final files
  |
  v
Create DB transaction/records
  |
  v
Remove temporary file
  |
  v
Return photo metadata
```

Failure must clean up temporary and partially generated files.

Never trust the client-provided MIME type alone.

Verify actual image content.

Reject unsupported/corrupt images.

Set hard upload limits.

Do not allow arbitrary file extensions.

---

# 10. Image Delivery

The public site must use responsive images.

Use:

```text
srcset
sizes
loading="lazy"
decoding="async"
```

Use eager loading only for critical above-the-fold images.

Every image should reserve its dimensions to prevent CLS.

Example conceptual output:

```html
<img
  src="medium.webp"
  srcset="
    small.webp 768w,
    medium.webp 1280w,
    large.webp 2400w
  "
  sizes="..."
  width="..."
  height="..."
  loading="lazy"
  decoding="async"
/>
```

Do not load 12 MB originals into gallery cards.

---

# 11. Image Format Strategy

Initial strategy:

```text
Original:
    Preserve original uploaded file.

Web delivery:
    WebP variants.

Optional later:
    AVIF after verifying hosting/server/library support.
```

Do not destroy the original.

Do not repeatedly recompress already-generated variants.

Use deterministic image processing.

Strip sensitive EXIF/location information from public derivatives unless the user explicitly wants it exposed.

Keep private/original metadata separate from public metadata.

---

# 12. Public Website Routes

Suggested routes:

```text
/
 /about
 /photography
 /photography/:category
 /album/:slug
 /photo/:slug

 /projects
 /projects/:slug

 /experience
 /contact
 /resume
```

Admin:

```text
/admin/login
/admin
/admin/photos
/admin/photos/new
/admin/photos/:id/edit
/admin/albums
/admin/categories
/admin/tags
/admin/projects
/admin/messages
/admin/settings
```

Admin routes must not merely be hidden in the UI. API authorization must enforce access.

---

# 13. API Design

Use versioned API routes:

```text
/api/v1/...
```

Public:

```text
GET /api/v1/photos
GET /api/v1/photos/{slug}
GET /api/v1/albums
GET /api/v1/albums/{slug}
GET /api/v1/categories
GET /api/v1/projects
GET /api/v1/projects/{slug}
POST /api/v1/contact
GET /api/v1/site
```

Admin:

```text
POST   /api/v1/admin/photos
PUT    /api/v1/admin/photos/{id}
DELETE /api/v1/admin/photos/{id}

POST   /api/v1/admin/photos/bulk
POST   /api/v1/admin/photos/{id}/publish
POST   /api/v1/admin/photos/{id}/feature

POST   /api/v1/admin/albums
PUT    /api/v1/admin/albums/{id}
DELETE /api/v1/admin/albums/{id}

POST   /api/v1/admin/projects
PUT    /api/v1/admin/projects/{id}
DELETE /api/v1/admin/projects/{id}

GET    /api/v1/admin/messages
```

Use DTOs. Never expose EF entities directly.

---

# 14. Authentication

Only the site owner needs an admin account initially.

Use ASP.NET Core Identity.

Do not build custom password hashing.

Requirements:

- Strong password policy.
- Account lockout/rate limiting.
- Secure authentication cookies.
- Secure, HttpOnly cookies where cookie auth is used.
- Appropriate SameSite configuration.
- HTTPS only.
- Antiforgery/CSRF protection for cookie-authenticated state-changing requests.
- Avoid storing long-lived auth tokens in localStorage.
- Protect password-reset and login endpoints.
- Do not expose whether a username/email exists unnecessarily.

Use roles/policies:

```text
Administrator
Editor
```

Even if only Administrator exists initially.

ASP.NET Core explicitly separates authentication from authorization and supports role/policy-based authorization; use those mechanisms rather than implementing authorization manually.

---

# 15. Admin Security

Admin endpoints require authorization.

Example policy concept:

```text
[Authorize(Policy = "AdminOnly")]
```

The frontend is never the security boundary.

Never rely on:

```text
if (!isAdmin) hide button
```

as security.

Server must reject unauthorized requests.

Use a default-deny approach where practical so new sensitive endpoints do not accidentally become public.

---

# 16. CORS

If React and API are hosted on different origins during development:

```text
http://localhost:5173
```

may be allowed in Development.

Production should allow only the exact production frontend origin(s).

Never use:

```text
AllowAnyOrigin()
```

with credentials.

If production can be hosted under the same domain/origin, prefer that arrangement because it reduces CORS complexity.

---

# 17. Security Headers

Configure appropriate headers, including as applicable:

```text
Strict-Transport-Security
Content-Security-Policy
X-Content-Type-Options
Referrer-Policy
Permissions-Policy
```

Do not blindly copy a CSP from the internet.

Build CSP around the actual application dependencies.

Avoid unsafe-inline and unsafe-eval unless a specific dependency genuinely requires them.

Review CSP after deployment.

---

# 18. Input Security

Every user-controlled input is untrusted.

Validate on the server:

- strings
- slugs
- URLs
- descriptions
- file names
- uploaded images
- IDs
- pagination parameters
- sorting parameters
- filters

Prevent:

- SQL injection
- XSS
- path traversal
- malicious uploads
- open redirects
- excessive payloads
- enumeration where inappropriate

Never render arbitrary HTML from database content unless it is deliberately sanitized.

Prefer plain text/Markdown with a controlled sanitizer if rich text is later introduced.

---

# 19. File Upload Security

Allowed image formats should initially be restricted to:

```text
JPEG
PNG
WebP
```

Optionally add HEIC/HEIF only after verifying the hosting environment and processing library.

Set:

- Maximum request body size.
- Maximum individual file size.
- Maximum number of files per bulk upload.
- Maximum pixel dimensions.
- Processing timeout/resource safeguards.

Do not accept:

```text
.exe
.dll
.php
.js
.html
.svg
```

unless there is a very specific and reviewed reason.

Be careful with SVG because it can contain active content.

Store uploads outside executable web application paths where possible.

If files must be publicly served, serve only generated image variants from a controlled route/storage location.

---

# 20. EXIF and Privacy

EXIF can contain:

- GPS coordinates.
- Camera serial information.
- Date/time.
- Device information.

Therefore:

```text
Original/private metadata
        !=
Public metadata
```

The public site should expose only intentionally selected metadata.

GPS should not automatically be published.

Admin should control whether location metadata is public.

---

# 21. Rate Limiting

Rate-limit:

- Login.
- Password recovery.
- Contact form.
- Upload endpoints.
- Bulk operations.
- Expensive API endpoints.

Use ASP.NET Core rate limiting facilities where appropriate.

Do not rate-limit static image delivery so aggressively that legitimate gallery browsing becomes unpleasant.

---

# 22. Contact Form Security

The public contact endpoint must have:

- Server-side validation.
- Length limits.
- Rate limiting.
- Spam protection.
- Request size limits.
- Sanitized output.
- No direct HTML email injection.
- Logging without exposing sensitive data.

Start without a paid email service.

Store contact messages in SQL Server.

Admin can view/delete messages.

An email notification mechanism can be added later if a free/host-provided option is available.

---

# 23. React Architecture

Suggested structure:

```text
web/src/
|
+-- app/
|   +-- router/
|   +-- providers/
|   +-- config/
|
+-- components/
|   +-- ui/
|   +-- layout/
|   +-- gallery/
|   +-- photography/
|   +-- projects/
|
+-- features/
|   +-- photos/
|   +-- albums/
|   +-- projects/
|   +-- contact/
|   +-- admin/
|
+-- pages/
|
+-- hooks/
|
+-- lib/
|   +-- api/
|   +-- validation/
|
+-- types/
|
+-- styles/
```

Keep feature-specific logic close to the feature.

Avoid a giant:

```text
components/
```

folder containing the entire application.

---

# 24. UI/Design Direction

Design target:

- Premium.
- Minimal.
- Editorial.
- Photography-first.
- Strong typography.
- Large imagery.
- Generous whitespace.
- Subtle motion.
- Excellent dark mode.
- High contrast.
- No visual clutter.

Use Apple/Tesla-level product presentation as inspiration for:

- hierarchy
- whitespace
- typography
- restrained motion
- immersive imagery
- clarity

Do not copy their visual identity.

Also research high-end photography portfolio/editorial sites for ideas. The target should be a blend of:

```text
Premium product website
+
Editorial photography magazine
+
Professional developer portfolio
```

---

# 25. Responsive Design Strategy

Design mobile-first, then scale up.

Breakpoints should not be treated as the design itself.

The layout should adapt based on content.

Must support:

```text
320px mobile
375px
390px
430px
768px tablet
1024px laptop
1440px desktop
1920px desktop
2560px+
4K TV
```

For large screens:

- Increase content width intelligently.
- Do not stretch text to enormous line lengths.
- Use max-width containers.
- Allow photography grids to breathe.
- Use larger image presentations where appropriate.

For TV:

- Ensure navigation remains readable from distance.
- Avoid tiny controls.
- Maintain strong contrast.
- Do not require hover to understand important information.

---

# 26. Accessibility

Target WCAG 2.2 AA practices.

Requirements:

- Semantic HTML.
- Keyboard navigation.
- Visible focus states.
- Proper heading hierarchy.
- Accessible buttons.
- Accessible dialogs/lightboxes.
- Alt text.
- Reduced motion.
- Sufficient color contrast.
- Form labels and errors.
- Screen-reader-friendly gallery controls.

The photography lightbox must work without a mouse.

---

# 27. Performance Budget

Set explicit targets rather than saying "make it fast."

Goals:

- Minimal JavaScript on initial load.
- Lazy-load non-critical routes/components.
- Avoid huge third-party libraries.
- Optimize images.
- Cache static assets.
- Compress HTTP responses.
- Use long-lived immutable caching for versioned assets.
- Paginate APIs.
- Avoid unnecessary database queries.
- Avoid rendering hundreds of DOM nodes simultaneously.

Measure with:

- Lighthouse.
- Chrome DevTools.
- Web Vitals.
- Playwright performance checks where useful.

Target excellent Core Web Vitals:

```text
LCP: good
CLS: good
INP: good
```

Do not optimize blindly; measure before/after.

---

# 28. Gallery Performance

A photography gallery must not render hundreds of huge images.

Use:

```text
Pagination / cursor pagination
+
Responsive image variants
+
Lazy loading
+
Virtualization only if actually necessary
```

For the first version, normal pagination/infinite loading is preferable to prematurely adding complex virtualization.

---

# 29. Caching

Potential layers:

```text
Browser cache
    |
HTTP cache headers
    |
ASP.NET response/output caching where useful
    |
Database query optimization
```

Generated image variants are naturally cacheable.

Use immutable/versioned storage keys where practical.

Do not introduce Redis initially.

---

# 30. SEO

Public photography pages should have:

- Unique title.
- Meta description.
- Canonical URL.
- OpenGraph metadata.
- Social preview image.
- Semantic headings.
- Descriptive alt text.
- Sitemap.
- robots.txt.
- Clean URLs.
- Structured data where appropriate.

Potential URL:

```text
/photography/automotive/volkswagen-polo
```

instead of:

```text
/photo?id=123
```

---

# 31. Analytics and Privacy

Do not add analytics automatically.

First version can work without analytics.

If analytics is added later:

- Prefer privacy-conscious tooling.
- Do not collect unnecessary personal data.
- Document it in the privacy policy.
- Avoid tracking admin activity as public analytics.

---

# 32. Logging and Observability

Backend logs should capture:

- Application errors.
- Authentication failures.
- Upload failures.
- Processing failures.
- Important admin operations.

Do NOT log:

- passwords
- authentication tokens
- connection strings
- secrets
- sensitive EXIF unnecessarily
- full request bodies by default

Use structured logging.

Include correlation/request IDs where practical.

---

# 33. Error Handling

API must return consistent errors.

Prefer a standard problem-details style response.

Example conceptual response:

```json
{
  "type": "...",
  "title": "Validation failed",
  "status": 400,
  "detail": "...",
  "errors": {
    "title": ["Title is required."]
  }
}
```

Do not expose stack traces or internal exception details in production.

Frontend must have:

- route-level error handling
- API error handling
- upload failure handling
- empty states
- loading states
- retry states

---

# 34. Admin Upload UX

The admin should support:

```text
Single upload
+
Bulk upload
+
Drag and drop
+
Upload progress
+
Preview
+
Edit metadata
+
Assign album/category/tags
+
Publish/unpublish
+
Feature/unfeature
```

Bulk upload should not require manually editing each photo if the same metadata applies to all selected images.

---

# 35. Draft/Publish Model

Content should support:

```text
Draft
Published
```

Admin can upload/process photographs without immediately publishing them.

This prevents unfinished content from appearing publicly.

---

# 36. Soft Delete / Recovery

Do not immediately destroy important metadata from the database when an admin deletes a photo.

At minimum, design deletion so storage/database cleanup is deliberate.

Potential future state:

```text
Active
Archived
Deleted
```

For V1, a simpler published/unpublished model is acceptable, but the storage service must safely remove associated variants when permanent deletion occurs.

---

# 37. API Pagination

All collection endpoints should have bounded pagination.

Example:

```text
GET /api/v1/photos?page=1&pageSize=24
```

Server must enforce maximum page size.

Do not allow:

```text
?pageSize=999999999
```

Use cursor pagination later if gallery size makes it worthwhile.

---

# 38. Sorting and Filtering

Do not allow arbitrary SQL/order expressions from clients.

Use allow-listed values:

```text
sort=recent
sort=oldest
sort=featured
```

Likewise categories/tags must be resolved through controlled identifiers/slugs.

---

# 39. Deployment

Target:

```text
Plesk
|
+-- React static build
|
+-- ASP.NET Core API
|
+-- SQL Server
|
+-- File storage
```

The exact Plesk configuration should be documented after the hosting environment is inspected.

Do not assume Linux/Windows hosting until verified.

Do not assume the host supports the latest .NET/Node version until verified.

---

# 40. Environment Configuration

Use separate environments:

```text
Development
Test
Production
```

Configuration must come from environment-specific configuration/environment variables.

Never commit:

```text
connection strings
passwords
API keys
JWT secrets
admin credentials
private keys
```

Use .NET User Secrets locally where appropriate.

Production secrets must be stored using the hosting environment's secure configuration mechanism.

---

# 41. CI/CD

Later add GitHub Actions:

```text
git push
   |
   v
Build
   |
   +--> Frontend tests
   |
   +--> Backend tests
   |
   +--> Lint
   |
   +--> Build production artifacts
   |
   v
Deploy
```

Deployment must not occur if tests fail.

Do not make CI/CD the first task. Get the application stable locally first.

---

# 42. Backup Strategy

The website has two important categories of data:

```text
SQL database
+
Original photographs
```

Both need backups.

Database backup alone is insufficient.

Photo originals must be backed up separately.

Generated variants can be recreated from originals, so originals have the highest priority.

Document:

- backup frequency
- retention
- restore procedure
- storage location
- restore testing

A backup that has never been restored is not a proven backup.

---

# 43. Dependency Management

Before adding a package:

1. Confirm it solves a real problem.
2. Check license.
3. Check maintenance/activity.
4. Check security advisories.
5. Prefer established libraries.
6. Avoid duplicate libraries that solve the same problem.

Keep dependencies minimal.

---

# 44. Testing Strategy

## Unit tests

Test:

- slug generation
- metadata mapping
- validation
- authorization policies
- image variant calculations
- business rules

## Integration tests

Test:

- API endpoints
- database behavior
- authentication
- authorization
- upload pipeline
- contact form
- public/private separation

## E2E tests

At minimum:

```text
Visitor opens homepage
Visitor opens gallery
Visitor opens photo
Visitor opens project
Admin logs in
Admin uploads photo
Admin edits metadata
Admin publishes photo
Unauthorized visitor cannot access admin API
```

---

# 45. Security Testing Checklist

Before production:

```text
[ ] HTTPS enforced
[ ] Secure cookies
[ ] Admin endpoints protected
[ ] Default-deny authorization reviewed
[ ] CORS restricted
[ ] CSRF protection reviewed
[ ] Rate limiting enabled
[ ] Upload limits enabled
[ ] File type validation enabled
[ ] Path traversal protections tested
[ ] XSS protections tested
[ ] SQL injection protections tested
[ ] Security headers configured
[ ] Secrets removed from repository
[ ] Production error details disabled
[ ] Logs reviewed for secrets
[ ] Dependency vulnerabilities checked
[ ] Database permissions minimized
[ ] Backup tested
[ ] Restore tested
```

---

# 46. Development Phases

## Phase 0 — Architecture
- Create repository.
- Create architecture documentation.
- Decide domain name later.
- Verify hosting capabilities.
- Verify .NET/Node/SQL Server versions.
- Establish coding standards.

## Phase 1 — React Foundation
- React + TypeScript + Vite.
- Routing.
- Tailwind.
- Design system.
- Responsive layout.
- Homepage skeleton.
- Theme/dark mode.
- Accessibility foundation.

## Phase 2 — ASP.NET Core Foundation
- API.
- Domain.
- Application.
- Infrastructure.
- EF Core.
- SQL Server.
- Migrations.
- Health endpoint.
- Structured error handling.

## Phase 3 — Public Portfolio
- Home.
- About.
- Projects.
- Project detail.
- Contact.
- Resume.

## Phase 4 — Photography
- Photo model.
- Albums.
- Categories.
- Gallery.
- Photo detail.
- Lightbox.
- Responsive images.

## Phase 5 — Image Processing
- Upload endpoint.
- File validation.
- Metadata extraction.
- Variant generation.
- Storage abstraction.
- Cleanup/error handling.

## Phase 6 — Admin
- Identity.
- Login.
- Authorization.
- Dashboard.
- Photo management.
- Album management.
- Category/tag management.
- Project management.

## Phase 7 — Hardening
- Security headers.
- Rate limiting.
- CORS.
- CSRF.
- Upload security.
- Validation.
- Logging.
- Error handling.
- Dependency audit.

## Phase 8 — Performance
- Image optimization.
- caching.
- database indexes.
- pagination.
- lazy loading.
- bundle analysis.
- Core Web Vitals.

## Phase 9 — Testing
- Unit tests.
- Integration tests.
- E2E tests.
- Security regression tests.

## Phase 10 — Deployment
- Plesk deployment.
- HTTPS.
- Production database.
- Storage.
- backups.
- monitoring.
- CI/CD.

---

# 47. Future Features — Do Not Build Yet

Possible future capabilities:

```text
Private client galleries
Password-protected albums
Photo favourites
Client photo selection
Download originals
Photography journal/blog
Travel map
Location browsing
Before/after comparisons
Advanced search
Tag filtering
Photo statistics
Public comments
Multiple admin users
Editor role
Cloud/CDN storage
AVIF
Background processing queue
Image watermarking
```

These are deliberately outside V1.

---

# 48. Architectural Principles

Follow these principles throughout implementation:

1. Security is enforced on the server.
2. The frontend is never trusted.
3. Images are files, not database blobs.
4. Original images are preserved.
5. Generated variants are disposable.
6. Public metadata and private metadata are separated.
7. Business logic belongs in the backend/application layer.
8. React components should remain focused on UI.
9. DTOs cross API boundaries; entities do not.
10. Dependencies are abstractions where future replacement is realistic.
11. Prefer boring, maintainable architecture over unnecessary complexity.
12. Measure performance instead of guessing.
13. Validate at every trust boundary.
14. Minimize third-party dependencies.
15. Document decisions that materially affect future development.

---

# 49. Codex Working Rules

When implementing this repository:

- Read this specification before making architectural changes.
- Do not silently change the stack.
- Do not introduce paid infrastructure.
- Do not add a new dependency when an existing dependency or platform feature is sufficient.
- Do not skip security because the site is "only a portfolio."
- Do not expose secrets.
- Do not implement fake authentication.
- Do not bypass validation to make a feature work.
- Do not store image blobs in SQL Server.
- Do not load original high-resolution images in normal gallery views.
- Do not implement all future features prematurely.
- Keep commits/features logically isolated.
- Explain architectural changes before making major changes.
- Prefer incremental implementation.
- After each major phase, run tests/build/lint and fix regressions.
- Update documentation when architecture changes.
- Do not generate placeholder architecture that is impossible to deploy to the target environment.
- Verify hosting constraints before deployment-specific implementation.

---

# 50. Definition of Done

A feature is not complete merely because it works on the developer's machine.

A feature is complete when:

```text
[ ] Implementation works
[ ] Type checking passes
[ ] Linting passes
[ ] Tests pass
[ ] Error states exist
[ ] Loading states exist
[ ] Empty states exist where applicable
[ ] Responsive behavior verified
[ ] Accessibility considered
[ ] Security boundary reviewed
[ ] Performance impact considered
[ ] No secrets introduced
[ ] Documentation updated where necessary
```

---

# 51. First Codex Task

Do NOT immediately build the entire application.

First:

1. Inspect the repository and environment.
2. Confirm installed Node.js, npm, .NET SDK, SQL Server tooling, and Git versions.
3. Confirm target Plesk hosting capabilities separately.
4. Create the repository structure.
5. Create the React application.
6. Create the ASP.NET Core solution/projects.
7. Configure TypeScript/ESLint/Prettier.
8. Configure .NET analyzers and nullable reference types.
9. Create the initial architecture documentation.
10. Add a minimal health-check API.
11. Add a minimal React shell that can communicate with the API.
12. Verify local development.
13. Run frontend and backend tests/builds.
14. Stop and report the result.

Do not implement authentication, image upload, database models, or the full UI during the first task.

The purpose of the first task is to establish a clean foundation.

---

# 52. Important Design Philosophy

This project has two goals:

### Goal A — Build something genuinely impressive.

A visitor should immediately think:

> "This person is a photographer and a software developer."

### Goal B — Learn by building.

The developer should understand:

```text
React
TypeScript
API architecture
ASP.NET Core
EF Core
SQL Server
Authentication
Authorization
File storage
Image processing
Responsive design
Accessibility
Security
Performance
Testing
CI/CD
Deployment
```

Do not allow AI/Codex to turn the project into a black box.

When generating substantial code, explain:

- what was created
- why it exists
- how it connects to other components
- what security considerations apply
- how to test it
- what to learn from it

The developer should be able to maintain the entire application without depending on the AI.

---

# 53. Reference Security Documentation

Use current official ASP.NET Core security guidance when making security decisions:

- Authentication and authorization
- HTTPS
- Data Protection
- CSRF/XSRF
- CORS
- XSS
- secure secret storage
- role/policy authorization

Do not treat this document as a substitute for checking current framework documentation when implementing security-sensitive functionality.

---

# 54. Success Criteria

The finished system should be:

```text
Beautiful
+
Fast
+
Responsive
+
Accessible
+
Secure
+
Maintainable
+
Free/open-source oriented
+
Deployable on the available hosting
+
Educational for the developer
+
Expandable without architectural rewrites
```

The website is not merely a portfolio.

It is a real-world full-stack engineering project presented through a personal photography and software-development brand.
