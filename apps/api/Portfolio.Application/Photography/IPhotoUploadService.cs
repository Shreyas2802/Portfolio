namespace Portfolio.Application.Photography;

public interface IPhotoUploadService
{
    Task<UploadPhotoResult> UploadAsync(UploadPhotoRequest request, CancellationToken cancellationToken);

    /// <returns>false if no photo with that id exists.</returns>
    Task<bool> DeleteAsync(int photoId, CancellationToken cancellationToken);
}
