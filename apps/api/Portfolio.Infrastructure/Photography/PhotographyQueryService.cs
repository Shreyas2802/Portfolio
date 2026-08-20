using Microsoft.EntityFrameworkCore;
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
        var album = await _dbContext.Albums
            .Include(a => a.Category)
            .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
            .FirstOrDefaultAsync(a => a.Slug == slug && a.IsPublished, cancellationToken);

        if (album is null)
        {
            return null;
        }

        return new AlbumDetailDto
        {
            Id = album.Id,
            Title = album.Title,
            Slug = album.Slug,
            Description = album.Description,
            CategoryName = album.Category?.Name,
            Photos = album.Photos.Where(p => p.IsPublished).Select(ToDto).ToList(),
        };
    }

    public async Task<List<PhotoDto>> GetFeaturedPhotosAsync(int count, CancellationToken cancellationToken)
    {
        var photos = await _dbContext.Photos
            .Where(p => p.IsPublished)
            .OrderBy(p => p.DisplayOrder)
            .Take(count)
            .ToListAsync(cancellationToken);

        return photos.Select(ToDto).ToList();
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
    };
}
