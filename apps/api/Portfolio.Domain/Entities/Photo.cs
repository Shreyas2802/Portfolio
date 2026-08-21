namespace Portfolio.Domain.Entities;

/// <summary>
/// A single photo. ThumbnailPath/MediumPath/LargePath point at
/// pre-generated variants under wwwroot/uploads/photos - hand-generated
/// for the Phase 4 seed data. The automated version of this (upload ->
/// validate -> generate variants -> store) is Phase 5's job; this
/// entity's shape doesn't need to change when that pipeline exists,
/// it'll just populate these same fields instead of the seeder.
///
/// Technical fields (Aperture/ShutterSpeed/Iso/FocalLengthMm/etc.) are
/// deliberately real EXIF data pulled from the source files rather
/// than placeholders - this is exactly what the DataLabel UI device
/// displays on each photo.
/// </summary>
public class Photo
{
    public int Id { get; set; }
    public int AlbumId { get; set; }
    public Album? Album { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;

    public string ThumbnailPath { get; set; } = string.Empty;
    public string MediumPath { get; set; } = string.Empty;
    public string LargePath { get; set; } = string.Empty;
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

    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; }
    public DateTime UploadedAtUtc { get; set; }

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
