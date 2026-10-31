namespace Portfolio.Application.Photography;

public interface ICategoryManagementService
{
    Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken cancellationToken);

    /// <returns>false if no category with that id exists.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
