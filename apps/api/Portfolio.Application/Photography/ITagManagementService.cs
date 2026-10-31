namespace Portfolio.Application.Photography;

public interface ITagManagementService
{
    Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<TagDto> CreateAsync(TagRequest request, CancellationToken cancellationToken);

    /// <returns>false if no tag with that id exists.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
