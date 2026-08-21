using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Application.Auth;
using Portfolio.Application.Contact;
using Portfolio.Application.Photography;
using Portfolio.Infrastructure.Contact;
using Portfolio.Infrastructure.Identity;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Persistence.Seed;
using Portfolio.Infrastructure.Photography;

var builder = WebApplication.CreateBuilder(args);

// --- Services -------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

var jwtKey = builder.Configuration["Jwt:Key"];
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            // Empty key here is fine at startup (e.g. before user-secrets
            // are set) - token validation will simply fail closed on
            // every request rather than throwing at boot.
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? string.Empty)),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddScoped<IPhotographyQueryService, PhotographyQueryService>();
builder.Services.AddScoped<IPhotoStorageService, LocalPhotoStorageService>();
builder.Services.AddScoped<IPhotoUploadService, PhotoUploadService>();
builder.Services.AddScoped<IAuthService, AuthService>();

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
