using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Photography;

namespace Portfolio.Api.Controllers;

/// <summary>
/// Tag management only - assigning tags to individual photos isn't
/// wired into the upload/edit forms yet. This satisfies the spec's
/// "tag management" bullet at the entity level; per-photo tagging is
/// a natural follow-up once there's a photo-edit screen (today's admin
/// only supports upload + delete, not edit).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagManagementService _tagManagementService;

    public TagsController(ITagManagementService tagManagementService)
    {
        _tagManagementService = tagManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tags = await _tagManagementService.GetAllAsync(cancellationToken);
        return Ok(tags);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] TagRequest request, CancellationToken cancellationToken)
    {
        var tag = await _tagManagementService.CreateAsync(request, cancellationToken);
        return Ok(tag);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _tagManagementService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}
