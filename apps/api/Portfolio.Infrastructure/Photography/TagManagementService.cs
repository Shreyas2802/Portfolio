using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Photography;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Photography;

public class TagManagementService : ITagManagementService
{
    private readonly PortfolioDbContext _dbContext;

    public TagManagementService(PortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                PhotoCount = t.Photos.Count,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TagDto> CreateAsync(TagRequest request, CancellationToken cancellationToken)
    {
        var slug = await GenerateUniqueSlugAsync(request.Name, cancellationToken);
        var tag = new Tag { Name = request.Name, Slug = slug };
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TagDto { Id = tag.Id, Name = tag.Name, Slug = tag.Slug, PhotoCount = 0 };
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var tag = await _dbContext.Tags.FindAsync([id], cancellationToken);
        if (tag is null)
        {
            return false;
        }

        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.Slugify(name);
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "tag";
        }

        var slug = baseSlug;
        var suffix = 1;
        while (await _dbContext.Tags.AnyAsync(t => t.Slug == slug, cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }
        return slug;
    }
}
