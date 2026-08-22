using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.Photography;

/// <summary>
/// Input for the photo upload endpoint. File-level checks (is it
/// actually a decodable image, is it under the size limit) happen in
/// the upload service rather than via DataAnnotations, since those
/// require inspecting the file content, not just its declared type -
/// a client can label anything "image/jpeg" and be wrong.
/// </summary>
public class UploadPhotoRequest
{
    [Required]
    public Stream FileStream { get; set; } = Stream.Null;

    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Caption { get; set; } = string.Empty;

    [Required]
    public int AlbumId { get; set; }

    public List<int> TagIds { get; set; } = new();
}

public class UploadPhotoResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public PhotoDto? Photo { get; set; }
}
