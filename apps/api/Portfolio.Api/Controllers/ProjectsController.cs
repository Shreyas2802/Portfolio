using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Projects;
using Portfolio.Application.Validation;

namespace Portfolio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectQueryService _projectQueryService;
    private readonly IProjectManagementService _projectManagementService;

    public ProjectsController(
        IProjectQueryService projectQueryService,
        IProjectManagementService projectManagementService)
    {
        _projectQueryService = projectQueryService;
        _projectManagementService = projectManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublished(CancellationToken cancellationToken)
    {
        var projects = await _projectQueryService.GetPublishedAsync(cancellationToken);
        return Ok(projects);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var project = await _projectQueryService.GetBySlugAsync(slug, cancellationToken);
        if (project is null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllForAdmin(CancellationToken cancellationToken)
    {
        var projects = await _projectManagementService.GetAllForAdminAsync(cancellationToken);
        return Ok(projects);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ProjectRequest request, CancellationToken cancellationToken)
    {
        if (!IsValidUrls(request, out var error))
        {
            return BadRequest(new { error });
        }

        var project = await _projectManagementService.CreateAsync(request, cancellationToken);
        return Ok(project);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ProjectRequest request, CancellationToken cancellationToken)
    {
        if (!IsValidUrls(request, out var error))
        {
            return BadRequest(new { error });
        }

        var project = await _projectManagementService.UpdateAsync(id, request, cancellationToken);
        if (project is null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _projectManagementService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }

    private static bool IsValidUrls(ProjectRequest request, out string? error)
    {
        if (!SafeUrlValidator.IsSafe(request.GithubUrl) || !SafeUrlValidator.IsSafe(request.LiveUrl))
        {
            error = "GitHub URL and Live URL must be valid http:// or https:// links.";
            return false;
        }
        error = null;
        return true;
    }
}
