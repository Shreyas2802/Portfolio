using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Api.Middleware;
using Portfolio.Application.Admin;
using Portfolio.Application.Auth;
using Portfolio.Application.Contact;
using Portfolio.Application.Photography;
using Portfolio.Application.Projects;
using Portfolio.Infrastructure.Admin;
using Portfolio.Infrastructure.Contact;
using Portfolio.Infrastructure.Identity;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Persistence.Seed;
using Portfolio.Infrastructure.Photography;
using Portfolio.Infrastructure.Projects;

var builder = WebApplication.CreateBuilder(args);

// Global request body size cap - defense in depth beyond the
// per-endpoint [RequestSizeLimit] on Upload. Protects every endpoint
// (Contact, Login, etc.) from an oversized request body, not just the
// one already-guarded upload action.
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

// --- Services -------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

const string DevClientCorsPolicy = "DevClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevClientCorsPolicy, policy =>
    {
        // No AllowAnyOrigin(), no AllowCredentials() (auth is a JWT via
        // the Authorization header, not a cookie, so the credentialed-
        // CORS combination the spec warns against doesn't apply here).
        // Both origins point at the SAME app in production once the
        // built SPA is served from this same API (see MapFallbackToFile
        // below) - the production origin is kept in the list anyway as
        // a harmless safety net in case that ever changes, and the dev
        // origin is required since local dev genuinely is cross-origin
        // (Vite on 5173, API on 5199).
        policy.WithOrigins("http://localhost:5173", "https://portfolio.vistrut.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Matches the spec's explicit list: Login, Contact form, and
    // Upload are rate-limited; static image delivery and ordinary
    // GETs are deliberately left alone so gallery browsing stays
    // pleasant. Partitioned by IP so one abusive client can't exhaust
    // the limit for everyone else.
    options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));

    options.AddPolicy("contact", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromHours(1),
            QueueLimit = 0,
        }));

    options.AddPolicy("upload", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromHours(1),
            QueueLimit = 0,
        }));
});

builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // Reasonable defaults for a single-admin site - not configurable
        // by an end user, so "make it easy to type" doesn't apply the
        // way it would for public registration.
        options.Password.RequiredLength = 12;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<PortfolioDbContext>();

// JWT settings are read when the bearer options are first built, from
// the app's final IConfiguration - not captured into a local variable
// here. Reading them eagerly at this point would miss configuration
// added later in startup (e.g. WebApplicationFactory overrides in the
// integration tests), leaving an empty key.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, config) =>
    {
        var jwtKey = config["Jwt:Key"];

        // Fail closed: with no key configured, validate against a random
        // throwaway key so every token is rejected (401) - instead of
        // building a zero-length key, which throws and turns every
        // request into a 500.
        var signingKeyBytes = string.IsNullOrWhiteSpace(jwtKey)
            ? RandomNumberGenerator.GetBytes(32)
            : Encoding.UTF8.GetBytes(jwtKey);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddScoped<IPhotographyQueryService, PhotographyQueryService>();
builder.Services.AddScoped<IPhotoStorageService, LocalPhotoStorageService>();
builder.Services.AddScoped<IPhotoUploadService, PhotoUploadService>();
builder.Services.AddScoped<IAlbumManagementService, AlbumManagementService>();
builder.Services.AddScoped<ICategoryManagementService, CategoryManagementService>();
builder.Services.AddScoped<ITagManagementService, TagManagementService>();
builder.Services.AddScoped<IProjectQueryService, ProjectQueryService>();
builder.Services.AddScoped<IProjectManagementService, ProjectManagementService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminQueryService, AdminQueryService>();

// Storage:UploadsRootPath is optional - unset means "use wwwroot/uploads"
// (unchanged dev behavior). In production this should point outside the
// deployed app's publish folder - see the deployment guide.
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

builder.Services.Configure<WatermarkOptions>(builder.Configuration.GetSection("Watermark"));

// Compress JSON API responses - the spec's "Compress HTTP responses"
// bullet. Static image variants under /uploads are already compressed
// (JPEG) and explicitly excluded below - re-compressing already-
// compressed binary data wastes CPU for no size benefit.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Named output-cache policy applied per-endpoint via [OutputCache] on
// public read-only GETs. A separate, unnamed default policy is left
// alone (not applied globally) so nothing is cached unless explicitly
// opted in - admin endpoints and all writes must never be cached.
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("Public60s", policy => policy.Expire(TimeSpan.FromSeconds(60)));
});

// Structured error handling: unhandled exceptions become a standard
// RFC 7807 ProblemDetails JSON response instead of leaking a stack
// trace to the client. This is on from day one rather than bolted on
// right before deployment - see the spec's pre-production checklist
// item "production error details disabled", which this satisfies
// automatically once we're out of Development.
builder.Services.AddProblemDetails();

var app = builder.Build();

// --- Middleware pipeline ---------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Dev-only seed: populates the gallery with real (EXIF-backed)
    // photo data so Phase 4's UI has something real to render before
    // the upload pipeline (Phase 5) or admin CMS (Phase 6) exist. No
    // effect once either of those starts writing real rows - see
    // DbSeeder for the idempotency check.
    using var photoSeedScope = app.Services.CreateScope();
    var dbContext = photoSeedScope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
    await DbSeeder.SeedAsync(dbContext);
    await ProjectSeeder.SeedAsync(dbContext);
}
else
{
    app.UseExceptionHandler();
}

// Admin user seed runs in any environment (not gated by Development) -
// unlike the photo seed, an admin account needs to exist wherever this
// API actually runs. It's idempotent and does nothing if AdminUser
// credentials aren't configured - see IdentitySeeder.
using (var identitySeedScope = app.Services.CreateScope())
{
    var userManager = identitySeedScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = identitySeedScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = identitySeedScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await IdentitySeeder.SeedAsync(userManager, roleManager, builder.Configuration, logger);
}

app.UseMiddleware<SecurityHeadersMiddleware>();

// Also skipped in Testing - see the UseRateLimiter comment below for
// the general reasoning. This one specifically avoids a well-known
// WebApplicationFactory gotcha: its in-memory test client defaults to
// plain HTTP, and UseHttpsRedirection() would silently turn every
// test's expected response into a 307 redirect instead of the actual
// controller response, breaking assertions for reasons that have
// nothing to do with what each test is checking.
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseResponseCompression();

// Serves the built React SPA (index.html, /assets/*) once it's copied
// into wwwroot as part of the production build (see the deployment
// guide) - in local dev, wwwroot has no SPA build in it (Vite serves
// the SPA itself on a different port), so this simply has nothing to
// serve there and is harmless.
app.UseStaticFiles();

// Uploaded photo variants are served separately, from whatever path
// Storage:UploadsRootPath resolves to (defaults to wwwroot/uploads,
// see StorageOptions) - kept as its own mapping, rather than relying
// on the wwwroot mapping above, specifically so production can point
// this at a folder OUTSIDE the app's publish directory without any
// other code needing to change. Long-lived, immutable caching: a
// photo's thumb/medium/large.jpg never changes in place (delete +
// re-upload always creates a new slug folder, see Phase 5/6).
var uploadsRootPath = builder.Configuration["Storage:UploadsRootPath"];
var resolvedUploadsPath = string.IsNullOrWhiteSpace(uploadsRootPath)
    ? Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads")
    : uploadsRootPath;
Directory.CreateDirectory(resolvedUploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(resolvedUploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
    },
});

app.UseCors(DevClientCorsPolicy);

// Skipped specifically in the "Testing" environment (integration
// tests use WebApplicationFactory, which shares one app/DI instance -
// and therefore one rate-limiter state - across every request a test
// class makes). Rate limiting itself is already manually verified
// working in Phase 7; this avoids unrelated integration tests
// spuriously hitting 429 because they share a rate-limit bucket with
// sibling tests in the same class, not because of anything the test
// is actually asserting.
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}
app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Falls back to the SPA's index.html for any request that didn't match
// a controller route or an existing static file - this is what makes
// client-side routing (React Router) work when someone loads
// /projects or /admin/photos directly rather than navigating there
// from the homepage. Only meaningful once wwwroot actually contains a
// built SPA (production, per the deployment guide) - in local dev
// nothing ever hits this API on the root path (Vite handles that on
// its own port), so this has no effect there.
app.MapFallbackToFile("index.html");

app.Run();

// Top-level statements generate an internal Program class by default -
// WebApplicationFactory<Program> (used by integration tests) needs it
// to be at least as visible as the test project can see. This doesn't
// change any runtime behavior.
public partial class Program { }
