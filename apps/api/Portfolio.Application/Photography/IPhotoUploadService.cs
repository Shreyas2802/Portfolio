namespace Portfolio.Application.Photography;

public interface IPhotoUploadService
{
    Task<UploadPhotoResult> UploadAsync(UploadPhotoRequest request, CancellationToken cancellationToken);
}
