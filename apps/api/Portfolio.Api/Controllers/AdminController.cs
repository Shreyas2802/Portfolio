using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Admin;

namespace Portfolio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminQueryService _adminQueryService;

    public AdminController(IAdminQueryService adminQueryService)
    {
        _adminQueryService = adminQueryService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _adminQueryService.GetDashboardStatsAsync(cancellationToken);
        return Ok(stats);
    }
}
