using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Photography;

namespace Portfolio.Api.Controllers;

/// <summary>
/// Public, read-only photography endpoints. Write operations (create/
/// edit/delete photos and albums) arrive with the authenticated admin
/// app in Phase 6 - everything here is intentionally GET-only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PhotographyController : ControllerBase
{
    private readonly IPhotographyQueryService _photographyQueryService;

    public PhotographyController(IPhotographyQueryService photographyQueryService)
    {
        _photographyQueryService = photographyQueryService;
    }

    [HttpGet("albums")]
    public async Task<IActionResult> GetAlbums(CancellationToken cancellationToken)
    {
        var albums = await _photographyQueryService.GetAlbumsAsync(cancellationToken);
        return Ok(albums);
    }

    [HttpGet("albums/{slug}")]
    public async Task<IActionResult> GetAlbum(string slug, CancellationToken cancellationToken)
    {
        var album = await _photographyQueryService.GetAlbumBySlugAsync(slug, cancellationToken);
        if (album is null)
        {
            return NotFound();
        }
        return Ok(album);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured([FromQuery] int count, CancellationToken cancellationToken)
    {
        var take = count is > 0 and <= 24 ? count : 6;
        var photos = await _photographyQueryService.GetFeaturedPhotosAsync(take, cancellationToken);
        return Ok(photos);
    }
}
