var builder = WebApplication.CreateBuilder(args);

// --- Services -------------------------------------------------------
// Controllers only for now. Identity, EF Core's DbContext, CORS policy,
// and authentication/authorization schemes are added in later phases
// (Phase 2 onward) once there's an actual domain model and admin
// surface to protect. Adding them empty now would just be dead
// configuration nobody can verify works.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// --- Middleware pipeline ---------------------------------------------
if (app.Environment.IsDevelopment())
{
    // Detailed error pages are fine locally; production error details
    // being disabled is tracked explicitly in the spec's pre-production
    // checklist (section 45) and will be revisited before deployment.
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
