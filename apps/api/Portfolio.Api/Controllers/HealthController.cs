using Microsoft.AspNetCore.Mvc;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Api.Controllers;

/// <summary>
/// Public, unauthenticated health-check endpoint. Now checks real DB
/// connectivity (via CanConnectAsync) rather than returning a static
/// "ok" - Phase 0's version couldn't tell you anything was actually
/// wrong. Still returns nothing sensitive: no connection string, no
/// server details, no stack traces (spec: public endpoints must not
/// expose admin/internal information).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly PortfolioDbContext _dbContext;

    public HealthController(PortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

        return Ok(new
        {
            status = "ok",
            database = canConnect ? "connected" : "unreachable",
            timestampUtc = DateTime.UtcNow,
        });
    }
}
