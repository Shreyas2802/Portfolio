namespace Portfolio.Domain.Entities;

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

    /// <summary>
    /// Exactly one photo can be pinned as the homepage hero.
    /// The SetHero admin endpoint clears all others before setting this.
    /// Falls back to the first published photo if none is pinned.
    /// </summary>
    public bool IsHeroPhoto { get; set; }

    /// <summary>
    /// Comma-separated dominant hex colours sampled from the thumbnail
    /// on upload by the frontend via Canvas API, then saved back via
    /// the SetHero endpoint. Format: "#1a2b3c,#d4a040,#8c7060".
    /// Used to dynamically tint the page accent to match the hero image.
    /// </summary>
    public string? AccentPalette { get; set; }

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
