using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Photography;

namespace Portfolio.Api.Controllers;

/// <summary>
/// Public, read-only photography endpoints, plus one write endpoint
/// (upload) added in Phase 5. Real authentication/authorization
/// (Identity, Login, [Authorize] with roles) arrives in Phase 6 - until
/// then, upload is gated by a temporary shared-secret header
/// (X-Admin-Key) checked against configuration, so this doesn't sit
/// open with no protection at all in the gap between phases. This gate
/// is explicitly temporary: replace it with [Authorize(Roles = "Admin")]
/// once Identity lands, and remove the AdminUploadKey configuration
/// entirely at that point.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PhotographyController : ControllerBase
{
    private readonly IPhotographyQueryService _photographyQueryService;
    private readonly IPhotoUploadService _photoUploadService;
    private readonly IConfiguration _configuration;

    public PhotographyController(
        IPhotographyQueryService photographyQueryService,
        IPhotoUploadService photoUploadService,
        IConfiguration configuration)
    {
        _photographyQueryService = photographyQueryService;
        _photoUploadService = photoUploadService;
        _configuration = configuration;
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

    [HttpPost("upload")]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string title,
        [FromForm] string? caption,
        [FromForm] int albumId,
        CancellationToken cancellationToken)
    {
        var configuredKey = _configuration["AdminUploadKey"];
        var providedKey = Request.Headers["X-Admin-Key"].ToString();
        // Fail closed: if AdminUploadKey isn't configured at all, no
        // key can ever match, so upload is unreachable rather than
        // silently open - a missing config value should never mean
        // "no check happens".
        if (string.IsNullOrEmpty(configuredKey) || providedKey != configuredKey)
        {
            return Unauthorized(new { error = "Missing or invalid X-Admin-Key header." });
        }

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
            },
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Photo);
    }
}
