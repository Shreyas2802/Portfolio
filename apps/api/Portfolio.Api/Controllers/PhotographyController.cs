using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.Photography;

namespace Portfolio.Api.Controllers;

/// <summary>
/// Public, read-only photography endpoints, plus the upload endpoint.
/// Upload is now protected by real Identity-based authorization
/// (Phase 6) - this replaces the temporary X-Admin-Key header check
/// from Phase 5, which existed only because Identity didn't exist yet.
///
/// Public GET endpoints use a short (60s) output cache (Phase 8) - a
/// visitor browsing the gallery hits the cache, not the database, on
/// repeat requests within that window. Admin endpoints are never
/// cached, so changes made through the admin UI are always immediately
/// visible there.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PhotographyController : ControllerBase
{
    private readonly IPhotographyQueryService _photographyQueryService;
    private readonly IPhotoUploadService _photoUploadService;
    private readonly IAlbumManagementService _albumManagementService;

    public PhotographyController(
        IPhotographyQueryService photographyQueryService,
        IPhotoUploadService photoUploadService,
        IAlbumManagementService albumManagementService)
    {
        _photographyQueryService = photographyQueryService;
        _photoUploadService = photoUploadService;
        _albumManagementService = albumManagementService;
    }

    [HttpGet("albums")]
    [OutputCache(PolicyName = "Public60s")]
    public async Task<IActionResult> GetAlbums(CancellationToken cancellationToken)
    {
        var albums = await _photographyQueryService.GetAlbumsAsync(cancellationToken);
        return Ok(albums);
    }

    [HttpGet("albums/{slug}")]
    [OutputCache(PolicyName = "Public60s")]
    public async Task<IActionResult> GetAlbum(string slug, CancellationToken cancellationToken)
    {
        var album = await _photographyQueryService.GetAlbumBySlugAsync(slug, cancellationToken);
        if (album is null)
        {
            return NotFound();
        }
        return Ok(album);
    }

    [HttpGet("albums/{slug}/photos")]
    [OutputCache(PolicyName = "Public60s")]
    public async Task<IActionResult> GetAlbumPhotos(
        string slug, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken cancellationToken)
    {
        var result = await _photographyQueryService.GetAlbumPhotosAsync(slug, page, pageSize, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpGet("featured")]
    [OutputCache(PolicyName = "Public60s")]
    public async Task<IActionResult> GetFeatured([FromQuery] int count, CancellationToken cancellationToken)
    {
        var take = count is > 0 and <= 24 ? count : 6;
        var photos = await _photographyQueryService.GetFeaturedPhotosAsync(take, cancellationToken);
        return Ok(photos);
    }

    [HttpGet("photos")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllPhotos(
        [FromQuery] int page, [FromQuery] int pageSize, CancellationToken cancellationToken)
    {
        var result = await _photographyQueryService.GetAllPhotosAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("upload")]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string title,
        [FromForm] string? caption,
        [FromForm] int albumId,
        [FromForm] List<int>? tagIds,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "No file was uploaded." });
        }

        await using var stream = file.OpenReadStream();
        var result = await _photoUploadService.UploadAsync(
            new UploadPhotoRequest
            {
                FileStream = stream,
                Title = title,
                Caption = caption ?? string.Empty,
                AlbumId = albumId,
                TagIds = tagIds ?? new List<int>(),
            },
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Photo);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _photoUploadService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("albums/admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAlbumsForAdmin(CancellationToken cancellationToken)
    {
        var albums = await _albumManagementService.GetAllForAdminAsync(cancellationToken);
        return Ok(albums);
    }

    [HttpPost("albums")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAlbum([FromBody] AlbumCreateRequest request, CancellationToken cancellationToken)
    {
        var album = await _albumManagementService.CreateAsync(request, cancellationToken);
        return Ok(album);
    }

    [HttpPut("albums/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAlbum(int id, [FromBody] AlbumUpdateRequest request, CancellationToken cancellationToken)
    {
        var album = await _albumManagementService.UpdateAsync(id, request, cancellationToken);
        if (album is null)
        {
            return NotFound();
        }
        return Ok(album);
    }

    [HttpDelete("albums/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAlbum(int id, CancellationToken cancellationToken)
    {
        var deleted = await _albumManagementService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}
