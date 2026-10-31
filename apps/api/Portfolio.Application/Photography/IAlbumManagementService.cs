namespace Portfolio.Application.Photography;

public interface IAlbumManagementService
{
    Task<List<AlbumAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken);
    Task<AlbumAdminDto> CreateAsync(AlbumCreateRequest request, CancellationToken cancellationToken);
    Task<AlbumAdminDto?> UpdateAsync(int id, AlbumUpdateRequest request, CancellationToken cancellationToken);

    /// <returns>false if no album with that id exists.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
