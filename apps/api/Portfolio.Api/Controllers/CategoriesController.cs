using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Photography;

namespace Portfolio.Api.Controllers;

/// <summary>
/// Categories group albums (e.g. "Festival & Devotional"). Listing is
/// public (used to populate the album-create form's category picker
/// and any future public filtering); create/delete are admin-only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryManagementService _categoryManagementService;

    public CategoriesController(ICategoryManagementService categoryManagementService)
    {
        _categoryManagementService = categoryManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _categoryManagementService.GetAllAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryManagementService.CreateAsync(request, cancellationToken);
        return Ok(category);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _categoryManagementService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}
