using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common;
using Portfolio.Application.Photography;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Photography;

public class PhotographyQueryService : IPhotographyQueryService
{
    private readonly PortfolioDbContext _dbContext;

    public PhotographyQueryService(PortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AlbumSummaryDto>> GetAlbumsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Albums
            .Where(a => a.IsPublished)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new AlbumSummaryDto
            {
                Id = a.Id,
                Title = a.Title,
                Slug = a.Slug,
                Description = a.Description,
                CategoryName = a.Category != null ? a.Category.Name : null,
                CoverThumbnailUrl = a.Photos
                    .OrderBy(p => p.DisplayOrder)
                    .Select(p => p.ThumbnailPath)
                    .FirstOrDefault(),
                PhotoCount = a.Photos.Count(p => p.IsPublished),
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AlbumDetailDto?> GetAlbumBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        // Metadata only, deliberately no photos here - photos are
        // fetched separately via GetAlbumPhotosAsync, paginated. An
        // earlier version eagerly loaded every photo in the album on
        // this same call, which doesn't scale once an album holds
        // hundreds of photos (see spec's Gallery Performance section).
        return await _dbContext.Albums
            .Where(a => a.Slug == slug && a.IsPublished)
            .Select(a => new AlbumDetailDto
            {
                Id = a.Id,
                Title = a.Title,
                Slug = a.Slug,
                Description = a.Description,
                CategoryName = a.Category != null ? a.Category.Name : null,
                PhotoCount = a.Photos.Count(p => p.IsPublished),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<PhotoDto>?> GetAlbumPhotosAsync(
        string albumSlug, int page, int pageSize, CancellationToken cancellationToken)
    {
        var albumId = await _dbContext.Albums
            .Where(a => a.Slug == albumSlug && a.IsPublished)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (albumId is null)
        {
            return null;
        }

        page = PaginationLimits.ClampPage(page);
        pageSize = PaginationLimits.ClampPageSize(pageSize);

        var query = _dbContext.Photos
            .Include(p => p.Tags)
            .Where(p => p.AlbumId == albumId && p.IsPublished)
            .OrderBy(p => p.DisplayOrder);

        var totalCount = await query.CountAsync(cancellationToken);
        var photos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PhotoDto>
        {
            Items = photos.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public async Task<List<PhotoDto>> GetFeaturedPhotosAsync(int count, CancellationToken cancellationToken)
    {
        var photos = await _dbContext.Photos
            .Include(p => p.Tags)
            .Where(p => p.IsPublished)
            .OrderBy(p => p.DisplayOrder)
            .Take(count)
            .ToListAsync(cancellationToken);

        return photos.Select(ToDto).ToList();
    }

    public async Task<PagedResult<PhotoDto>> GetAllPhotosAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = PaginationLimits.ClampPage(page);
        pageSize = PaginationLimits.ClampPageSize(pageSize);

        var query = _dbContext.Photos
            .Include(p => p.Tags)
            .OrderByDescending(p => p.UploadedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var photos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PhotoDto>
        {
            Items = photos.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    private static PhotoDto ToDto(Photo p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Caption = p.Caption,
        ThumbnailUrl = p.ThumbnailPath,
        MediumUrl = p.MediumPath,
        LargeUrl = p.LargePath,
        Width = p.Width,
        Height = p.Height,
        TakenAtUtc = p.TakenAtUtc,
        CameraMake = p.CameraMake,
        CameraModel = p.CameraModel,
        LensModel = p.LensModel,
        Aperture = p.Aperture,
        ShutterSpeed = p.ShutterSpeed,
        Iso = p.Iso,
        FocalLengthMm = p.FocalLengthMm,
        TagNames = p.Tags.Select(t => t.Name).ToList(),
    };
}
