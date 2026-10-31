using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Photography;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Photography;

public class CategoryManagementService : ICategoryManagementService
{
    private readonly PortfolioDbContext _dbContext;

    public CategoryManagementService(PortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                AlbumCount = c.Albums.Count,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken cancellationToken)
    {
        var slug = await GenerateUniqueSlugAsync(request.Name, cancellationToken);
        var category = new Category { Name = request.Name, Slug = slug };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CategoryDto { Id = category.Id, Name = category.Name, Slug = category.Slug, AlbumCount = 0 };
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories.FindAsync([id], cancellationToken);
        if (category is null)
        {
            return false;
        }

        // Deleting a category doesn't delete its albums - Album.CategoryId
        // is configured with DeleteBehavior.SetNull, so albums just
        // become uncategorized rather than disappearing.
        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.Slugify(name);
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "category";
        }

        var slug = baseSlug;
        var suffix = 1;
        while (await _dbContext.Categories.AnyAsync(c => c.Slug == slug, cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }
        return slug;
    }
}
