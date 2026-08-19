using Microsoft.AspNetCore.Mvc;

namespace Portfolio.Api.Controllers;

/// <summary>
/// Public, unauthenticated health-check endpoint. This exists purely
/// so the React SPA (and later, deployment monitoring) has something
/// to ping to confirm the API process is up and responding.
///
/// Security note: this deliberately returns nothing sensitive - no
/// connection strings, no version details, no stack traces. That
/// matters here because health endpoints are one of the more common
/// places people accidentally leak internal information (spec
/// principle: "public endpoints must not expose admin functionality").
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            timestampUtc = DateTime.UtcNow,
        });
    }
}
