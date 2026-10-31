namespace Portfolio.Application.Projects;

public interface IProjectQueryService
{
    Task<List<ProjectDto>> GetPublishedAsync(CancellationToken cancellationToken);
    Task<ProjectDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
}
