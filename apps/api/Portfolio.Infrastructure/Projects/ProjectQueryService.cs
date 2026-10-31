using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Projects;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Projects;

public class ProjectQueryService : IProjectQueryService
{
    private readonly PortfolioDbContext _dbContext;

    public ProjectQueryService(PortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProjectDto>> GetPublishedAsync(CancellationToken cancellationToken)
    {
        var projects = await _dbContext.Projects
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.IsFeatured)
            .ThenBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        return projects.Select(ToDto).ToList();
    }

    public async Task<ProjectDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished, cancellationToken);
        return project is null ? null : ToDto(project);
    }

    internal static ProjectDto ToDto(Project p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Tagline = p.Tagline,
        Role = p.Role,
        Status = p.Status,
        Overview = p.Overview,
        Contributions = SplitLines(p.ContributionsText),
        Technologies = SplitCsv(p.Technologies),
        GithubUrl = p.GithubUrl,
        LiveUrl = p.LiveUrl,
        CoverImageUrl = p.CoverImageUrl,
        IsFeatured = p.IsFeatured,
    };

    internal static List<string> SplitLines(string text) =>
        text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    internal static List<string> SplitCsv(string text) =>
        text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
