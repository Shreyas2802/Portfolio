namespace Portfolio.Application.Photography;

/// <summary>
/// What the frontend actually needs for a photo - never the raw
/// entity (spec principle #9: DTOs cross the API boundary, not
/// entities). Camera fields are nullable since not every future photo
/// will have EXIF (e.g. scanned film).
/// </summary>
public class PhotoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string MediumUrl { get; set; } = string.Empty;
    public string LargeUrl { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime? TakenAtUtc { get; set; }
    public string? CameraMake { get; set; }
    public string? CameraModel { get; set; }
    public string? LensModel { get; set; }
    public decimal? Aperture { get; set; }
    public string? ShutterSpeed { get; set; }
    public int? Iso { get; set; }
    public decimal? FocalLengthMm { get; set; }
    public List<string> TagNames { get; set; } = new();
}
