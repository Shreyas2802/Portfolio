using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Contact;
using Portfolio.Application.Photography;
using Portfolio.Infrastructure.Contact;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Persistence.Seed;
using Portfolio.Infrastructure.Photography;

var builder = WebApplication.CreateBuilder(args);

// --- Services -------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddScoped<IPhotographyQueryService, PhotographyQueryService>();

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
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
    await DbSeeder.SeedAsync(dbContext);
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();

app.Run();
