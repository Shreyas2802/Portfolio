namespace Portfolio.Application.Photography;

public interface IPhotographyQueryService
{
    Task<List<AlbumSummaryDto>> GetAlbumsAsync(CancellationToken cancellationToken);
    Task<AlbumDetailDto?> GetAlbumBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<List<PhotoDto>> GetFeaturedPhotosAsync(int count, CancellationToken cancellationToken);
}
