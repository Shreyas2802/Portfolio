using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Photography;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Photography;

public class AlbumManagementService : IAlbumManagementService
{
    private readonly PortfolioDbContext _dbContext;
    private readonly IPhotoStorageService _storageService;

    public AlbumManagementService(PortfolioDbContext dbContext, IPhotoStorageService storageService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
    }

    public async Task<List<AlbumAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Albums
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new AlbumAdminDto
            {
                Id = a.Id,
                Title = a.Title,
                Slug = a.Slug,
                Description = a.Description,
                CategoryId = a.CategoryId,
                CategoryName = a.Category != null ? a.Category.Name : null,
                IsPublished = a.IsPublished,
                PhotoCount = a.Photos.Count,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AlbumAdminDto> CreateAsync(AlbumCreateRequest request, CancellationToken cancellationToken)
    {
        var slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken);
        var maxOrder = await _dbContext.Albums.MaxAsync(a => (int?)a.DisplayOrder, cancellationToken) ?? -1;

        var album = new Album
        {
            Title = request.Title,
            Slug = slug,
            Description = request.Description,
            CategoryId = request.CategoryId,
            IsPublished = request.IsPublished,
            DisplayOrder = maxOrder + 1,
            CreatedAtUtc = DateTime.UtcNow,
        };

        _dbContext.Albums.Add(album);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(album.Id, cancellationToken);
    }

    public async Task<AlbumAdminDto?> UpdateAsync(int id, AlbumUpdateRequest request, CancellationToken cancellationToken)
    {
        var album = await _dbContext.Albums.FindAsync([id], cancellationToken);
        if (album is null)
        {
            return null;
        }

        // Slug is intentionally NOT regenerated on rename - the album's
        // URL should stay stable once published, even if the title
        // changes later. A title-only edit shouldn't break a link
        // someone already shared or bookmarked.
        album.Title = request.Title;
        album.Description = request.Description;
        album.CategoryId = request.CategoryId;
        album.IsPublished = request.IsPublished;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(album.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var album = await _dbContext.Albums
            .Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (album is null)
        {
            return false;
        }

        // Delete each photo's files before the DB cascade-delete removes
        // the rows - otherwise the files become orphaned on disk with
        // nothing pointing at them anymore.
        foreach (var photo in album.Photos)
        {
            foreach (var path in new[] { photo.ThumbnailPath, photo.MediumPath, photo.LargePath })
            {
                try
                {
                    await _storageService.DeleteVariantAsync(path, cancellationToken);
                }
                catch
                {
                    // Best-effort - a missing file shouldn't block deleting the album.
                }
            }
        }

        _dbContext.Albums.Remove(album);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AlbumAdminDto> ToDtoAsync(int albumId, CancellationToken cancellationToken)
    {
        var dto = await _dbContext.Albums
            .Where(a => a.Id == albumId)
            .Select(a => new AlbumAdminDto
            {
                Id = a.Id,
                Title = a.Title,
                Slug = a.Slug,
                Description = a.Description,
                CategoryId = a.CategoryId,
                CategoryName = a.Category != null ? a.Category.Name : null,
                IsPublished = a.IsPublished,
                PhotoCount = a.Photos.Count,
            })
            .FirstAsync(cancellationToken);
        return dto;
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.Slugify(title);
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "album";
        }

        var slug = baseSlug;
        var suffix = 1;
        while (await _dbContext.Albums.AnyAsync(a => a.Slug == slug, cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }
        return slug;
    }
}
