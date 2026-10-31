using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Photography;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhotographyController : ControllerBase
{
    private readonly IPhotographyQueryService _photographyQueryService;
    private readonly IPhotoUploadService _photoUploadService;
    private readonly IAlbumManagementService _albumManagementService;
    private readonly PortfolioDbContext _dbContext;

    public PhotographyController(
        IPhotographyQueryService photographyQueryService,
        IPhotoUploadService photoUploadService,
        IAlbumManagementService albumManagementService,
        PortfolioDbContext dbContext)
    {
        _photographyQueryService = photographyQueryService;
        _photoUploadService = photoUploadService;
        _albumManagementService = albumManagementService;
        _dbContext = dbContext;
    }

    // ── Public read endpoints ────────────────────────────────────────

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
        if (album is null) return NotFound();
        return Ok(album);
    }

    [HttpGet("albums/{slug}/photos")]
    [OutputCache(PolicyName = "Public60s")]
    public async Task<IActionResult> GetAlbumPhotos(
        string slug, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken cancellationToken)
    {
        var result = await _photographyQueryService.GetAlbumPhotosAsync(slug, page, pageSize, cancellationToken);
        if (result is null) return NotFound();
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

    /// <summary>
    /// Returns the pinned hero photo (IsHeroPhoto=true), or the first
    /// published photo by DisplayOrder if none is pinned.
    /// Used by the homepage to set the full-bleed background and
    /// extract the accent palette.
    /// </summary>
    [HttpGet("hero")]
    [OutputCache(PolicyName = "Public60s")]
    public async Task<IActionResult> GetHero(CancellationToken cancellationToken)
    {
        var hero = await _photographyQueryService.GetHeroPhotoAsync(cancellationToken);
        if (hero is null) return NoContent();
        return Ok(hero);
    }

    // ── Admin endpoints ──────────────────────────────────────────────

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
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string title,
        [FromForm] string? caption,
        [FromForm] int albumId,
        [FromForm] List<int>? tagIds,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file was uploaded." });

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
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Photo);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _photoUploadService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Pins a photo as the homepage hero and saves its accent palette.
    /// Clears IsHeroPhoto on every other photo first — only one hero
    /// at a time. The palette is extracted client-side via Canvas API
    /// and posted here so it's persisted and served on the hero
    /// endpoint (avoids re-extracting on every page load).
    /// </summary>
    [HttpPut("{id:int}/set-hero")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetHero(
        int id,
        [FromBody] SetHeroRequest request,
        CancellationToken cancellationToken)
    {
        var photo = await _dbContext.Photos.FindAsync(new object[] { id }, cancellationToken);
        if (photo is null) return NotFound();

        // Clear existing hero
        var currentHeroes = _dbContext.Photos.Where(p => p.IsHeroPhoto);
        await foreach (var p in currentHeroes.AsAsyncEnumerable().WithCancellation(cancellationToken))
            p.IsHeroPhoto = false;

        photo.IsHeroPhoto = true;
        photo.AccentPalette = request.AccentPalette;

        await _dbContext.SaveChangesAsync(cancellationToken);
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
        if (album is null) return NotFound();
        return Ok(album);
    }

    [HttpDelete("albums/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAlbum(int id, CancellationToken cancellationToken)
    {
        var deleted = await _albumManagementService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

public record SetHeroRequest(string? AccentPalette);
