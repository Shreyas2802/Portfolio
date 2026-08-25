using Portfolio.Application.Common;

namespace Portfolio.Application.Photography;

public interface IPhotographyQueryService
{
    Task<List<AlbumSummaryDto>> GetAlbumsAsync(CancellationToken cancellationToken);
    Task<AlbumDetailDto?> GetAlbumBySlugAsync(string slug, CancellationToken cancellationToken);

    /// <summary>Bounded, paginated - page/pageSize are clamped server-side (see PaginationLimits).</summary>
    Task<PagedResult<PhotoDto>?> GetAlbumPhotosAsync(string albumSlug, int page, int pageSize, CancellationToken cancellationToken);

    Task<List<PhotoDto>> GetFeaturedPhotosAsync(int count, CancellationToken cancellationToken);

    /// <summary>Admin-only: every photo, published or not, across all albums. Paginated.</summary>
    Task<PagedResult<PhotoDto>> GetAllPhotosAsync(int page, int pageSize, CancellationToken cancellationToken);
}
