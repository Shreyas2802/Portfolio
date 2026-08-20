using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Contact;
using Portfolio.Infrastructure.Contact;
using Portfolio.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- Services -------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IContactMessageService, ContactMessageService>();

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
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
