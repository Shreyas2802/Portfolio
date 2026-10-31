using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Projects;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Photography;

namespace Portfolio.Infrastructure.Projects;

public class ProjectManagementService : IProjectManagementService
{
    private readonly PortfolioDbContext _dbContext;

    public ProjectManagementService(PortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProjectAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken)
    {
        var projects = await _dbContext.Projects
            .OrderByDescending(p => p.IsFeatured)
            .ThenBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        return projects.Select(ToAdminDto).ToList();
    }

    public async Task<ProjectAdminDto> CreateAsync(ProjectRequest request, CancellationToken cancellationToken)
    {
        var slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken);
        var maxOrder = await _dbContext.Projects.MaxAsync(p => (int?)p.DisplayOrder, cancellationToken) ?? -1;

        var project = new Project
        {
            Title = request.Title,
            Slug = slug,
            Tagline = request.Tagline,
            Role = request.Role,
            Status = request.Status,
            Overview = request.Overview,
            ContributionsText = request.ContributionsText,
            Technologies = request.Technologies,
            GithubUrl = string.IsNullOrWhiteSpace(request.GithubUrl) ? null : request.GithubUrl,
            LiveUrl = string.IsNullOrWhiteSpace(request.LiveUrl) ? null : request.LiveUrl,
            IsFeatured = request.IsFeatured,
            IsPublished = request.IsPublished,
            DisplayOrder = maxOrder + 1,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToAdminDto(project);
    }

    public async Task<ProjectAdminDto?> UpdateAsync(int id, ProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FindAsync([id], cancellationToken);
        if (project is null)
        {
            return null;
        }

        // Slug is intentionally NOT regenerated on rename - same
        // reasoning as Album (see Phase 6c): a published project's URL
        // shouldn't break just because the title changed.
        project.Title = request.Title;
        project.Tagline = request.Tagline;
        project.Role = request.Role;
        project.Status = request.Status;
        project.Overview = request.Overview;
        project.ContributionsText = request.ContributionsText;
        project.Technologies = request.Technologies;
        project.GithubUrl = string.IsNullOrWhiteSpace(request.GithubUrl) ? null : request.GithubUrl;
        project.LiveUrl = string.IsNullOrWhiteSpace(request.LiveUrl) ? null : request.LiveUrl;
        project.IsFeatured = request.IsFeatured;
        project.IsPublished = request.IsPublished;
        project.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToAdminDto(project);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FindAsync([id], cancellationToken);
        if (project is null)
        {
            return false;
        }

        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ProjectAdminDto ToAdminDto(Project p)
    {
        var dto = ProjectQueryService.ToDto(p);
        return new ProjectAdminDto
        {
            Id = dto.Id,
            Title = dto.Title,
            Slug = dto.Slug,
            Tagline = dto.Tagline,
            Role = dto.Role,
            Status = dto.Status,
            Overview = dto.Overview,
            Contributions = dto.Contributions,
            Technologies = dto.Technologies,
            GithubUrl = dto.GithubUrl,
            LiveUrl = dto.LiveUrl,
            CoverImageUrl = dto.CoverImageUrl,
            IsFeatured = dto.IsFeatured,
            IsPublished = p.IsPublished,
        };
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.Slugify(title);
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "project";
        }

        var slug = baseSlug;
        var suffix = 1;
        while (await _dbContext.Projects.AnyAsync(p => p.Slug == slug, cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }
        return slug;
    }
}
