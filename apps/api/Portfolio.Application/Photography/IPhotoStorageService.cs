namespace Portfolio.Application.Photography;

/// <summary>
/// Storage abstraction (spec Phase 5 requirement): the upload service
/// depends on this interface, never on "write to wwwroot" directly.
/// Today's implementation is local disk; swapping to cloud storage
/// later (e.g. S3-compatible, Azure Blob) means writing one new
/// Infrastructure class, not touching the upload service or any
/// controller.
/// </summary>
public interface IPhotoStorageService
{
    /// <summary>
    /// Saves a variant's bytes under the given relative path (e.g.
    /// "photos/kasba-ganapati/large.jpg") and returns the URL path the
    /// frontend should use to load it (e.g. "/uploads/photos/kasba-
    /// ganapati/large.jpg").
    /// </summary>
    Task<string> SaveVariantAsync(string relativePath, byte[] content, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a previously saved variant, given the URL path
    /// SaveVariantAsync returned. Used for cleanup when an upload
    /// fails partway through (spec Phase 5: "Cleanup/error handling") -
    /// never leave orphaned files after a failed upload.
    /// </summary>
    Task DeleteVariantAsync(string urlPath, CancellationToken cancellationToken);
}
