namespace Portfolio.Application.Projects;

public interface IProjectManagementService
{
    Task<List<ProjectAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken);
    Task<ProjectAdminDto> CreateAsync(ProjectRequest request, CancellationToken cancellationToken);
    Task<ProjectAdminDto?> UpdateAsync(int id, ProjectRequest request, CancellationToken cancellationToken);

    /// <returns>false if no project with that id exists.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
