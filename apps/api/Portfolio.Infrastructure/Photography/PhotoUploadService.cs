using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Photography;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;
using SkiaSharp;

namespace Portfolio.Infrastructure.Photography;

/// <summary>
/// Handles the full upload pipeline: validate, extract EXIF,
/// generate three size variants, apply watermark to medium + large,
/// persist, and clean up on failure.
///
/// Watermark: bottom-right text overlay using SkiaSharp. Applied only
/// to medium and large variants — thumbs are too small for it to read.
/// Controlled by WatermarkOptions (enabled flag + text). SkiaSharp
/// handles the text rendering natively; no external font file needed
/// for the default system font, but we embed a fallback to ensure
/// consistent rendering across Windows and Linux (Plesk).
/// </summary>
public class PhotoUploadService : IPhotoUploadService
{
    private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB
    private static readonly (string Name, int MaxWidth)[] Variants =
    {
        ("thumb",  480),
        ("medium", 1200),
        ("large",  2400),
    };

    // Variants that get a watermark (thumb is too small)
    private static readonly HashSet<string> WatermarkedVariants = ["medium", "large"];

    private readonly PortfolioDbContext _dbContext;
    private readonly IPhotoStorageService _storageService;
    private readonly ILogger<PhotoUploadService> _logger;
    private readonly WatermarkOptions _watermark;

    public PhotoUploadService(
        PortfolioDbContext dbContext,
        IPhotoStorageService storageService,
        ILogger<PhotoUploadService> logger,
        IOptions<WatermarkOptions> watermarkOptions)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _logger = logger;
        _watermark = watermarkOptions.Value;
    }

    public async Task<UploadPhotoResult> UploadAsync(
        UploadPhotoRequest request, CancellationToken cancellationToken)
    {
        var album = await _dbContext.Albums.FindAsync([request.AlbumId], cancellationToken);
        if (album is null)
            return Fail("Album not found.");

        using var ms = new MemoryStream();
        await request.FileStream.CopyToAsync(ms, cancellationToken);
        var bytes = ms.ToArray();

        if (bytes.Length == 0)
            return Fail("Uploaded file is empty.");

        if (bytes.Length > MaxFileSizeBytes)
            return Fail($"File exceeds the {MaxFileSizeBytes / (1024 * 1024)} MB limit.");

        var decoded = SKBitmap.Decode(bytes);
        if (decoded is null)
            return Fail(
                "Couldn't read this file as an image. Supported formats: JPEG, PNG, WEBP. " +
                "HEIC and RAW formats aren't supported — convert to JPEG first.");

        if ((long)decoded.Width * decoded.Height > 50_000_000L)
        {
            decoded.Dispose();
            return Fail("Image dimensions are too large to process (max ~50 megapixels).");
        }

        try
        {
            var exif = ExtractExif(bytes);
            var oriented = ApplyExifOrientation(decoded, exif.Orientation);
            try
            {
                return await SaveUploadAsync(request, album, oriented, exif, cancellationToken);
            }
            finally
            {
                if (!ReferenceEquals(oriented, decoded))
                    oriented.Dispose();
            }
        }
        finally
        {
            decoded.Dispose();
        }
    }

    private async Task<UploadPhotoResult> SaveUploadAsync(
        UploadPhotoRequest request, Album album,
        SKBitmap original, ExifData exif,
        CancellationToken cancellationToken)
    {
        var slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken);
        var savedPaths = new List<string>();

        try
        {
            var paths = new Dictionary<string, string>();
            foreach (var (name, maxWidth) in Variants)
            {
                var bitmap = ResizeVariant(original, maxWidth);
                try
                {
                    // Apply watermark to medium and large — not thumb
                    if (_watermark.Enabled && WatermarkedVariants.Contains(name))
                        ApplyWatermark(bitmap, _watermark.Text);

                    var variantBytes = Encode(bitmap);
                    var relativePath = $"photos/{slug}/{name}.jpg";
                    var urlPath = await _storageService.SaveVariantAsync(
                        relativePath, variantBytes, cancellationToken);
                    savedPaths.Add(urlPath);
                    paths[name] = urlPath;
                }
                finally
                {
                    if (!ReferenceEquals(bitmap, original))
                        bitmap.Dispose();
                }
            }

            var photo = new Photo
            {
                AlbumId = album.Id,
                Title = request.Title,
                Slug = slug,
                Caption = request.Caption,
                ThumbnailPath = paths["thumb"],
                MediumPath = paths["medium"],
                LargePath = paths["large"],
                Width = original.Width,
                Height = original.Height,
                TakenAtUtc = exif.TakenAt,
                CameraMake = exif.CameraMake,
                CameraModel = exif.CameraModel,
                LensModel = exif.LensModel,
                Aperture = exif.Aperture,
                ShutterSpeed = exif.ShutterSpeed,
                Iso = exif.Iso,
                FocalLengthMm = exif.FocalLengthMm,
                DisplayOrder = await _dbContext.Photos
                    .CountAsync(p => p.AlbumId == album.Id, cancellationToken),
                IsPublished = true,
                UploadedAtUtc = DateTime.UtcNow,
            };

            if (request.TagIds.Count > 0)
            {
                var tags = await _dbContext.Tags
                    .Where(t => request.TagIds.Contains(t.Id))
                    .ToListAsync(cancellationToken);
                foreach (var tag in tags)
                    photo.Tags.Add(tag);
            }

            _dbContext.Photos.Add(photo);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new UploadPhotoResult
            {
                Success = true,
                Photo = new PhotoDto
                {
                    Id = photo.Id,
                    Title = photo.Title,
                    Slug = photo.Slug,
                    Caption = photo.Caption,
                    ThumbnailUrl = photo.ThumbnailPath,
                    MediumUrl = photo.MediumPath,
                    LargeUrl = photo.LargePath,
                    Width = photo.Width,
                    Height = photo.Height,
                    TakenAtUtc = photo.TakenAtUtc,
                    CameraMake = photo.CameraMake,
                    CameraModel = photo.CameraModel,
                    LensModel = photo.LensModel,
                    Aperture = photo.Aperture,
                    ShutterSpeed = photo.ShutterSpeed,
                    Iso = photo.Iso,
                    FocalLengthMm = photo.FocalLengthMm,
                    TagNames = photo.Tags.Select(t => t.Name).ToList(),
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Photo upload failed after writing {Count} variant(s); cleaning up",
                savedPaths.Count);
            foreach (var path in savedPaths)
            {
                try { await _storageService.DeleteVariantAsync(path, cancellationToken); }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx,
                        "Failed to clean up orphaned variant {Path}", path);
                }
            }
            return Fail("Upload failed while saving the photo. No files were left behind.");
        }
    }

    // ── Watermark ────────────────────────────────────────────────────

    /// <summary>
    /// Draws a subtle bottom-right watermark onto the bitmap in place.
    /// Font size scales with image width so it stays proportional across
    /// medium (1200px) and large (2400px) variants.
    /// Two layers: a slightly larger semi-transparent shadow pass, then
    /// the white text on top — gives legibility on both light and dark
    /// areas of the photo without a heavy box.
    /// </summary>
    private static void ApplyWatermark(SKBitmap bitmap, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        using var canvas = new SKCanvas(bitmap);

        float fontSize = bitmap.Width * 0.018f; // ~1.8% of width
        float margin   = bitmap.Width * 0.022f;

        // Shadow pass
        using var shadowPaint = new SKPaint
        {
            Color       = new SKColor(0, 0, 0, 100),
            TextSize    = fontSize,
            IsAntialias = true,
            Typeface    = SKTypeface.Default,
        };
        float textWidth  = shadowPaint.MeasureText(text);
        float x = bitmap.Width  - textWidth - margin + 1f;
        float y = bitmap.Height - margin    + 1f;
        canvas.DrawText(text, x, y, shadowPaint);

        // Text pass
        using var textPaint = new SKPaint
        {
            Color       = new SKColor(255, 255, 255, 200),
            TextSize    = fontSize,
            IsAntialias = true,
            Typeface    = SKTypeface.Default,
        };
        textWidth = textPaint.MeasureText(text);
        x = bitmap.Width - textWidth - margin;
        y = bitmap.Height - margin;
        canvas.DrawText(text, x, y, textPaint);

        canvas.Flush();
    }

    // ── Resize / encode ──────────────────────────────────────────────

    private static SKBitmap ResizeVariant(SKBitmap original, int maxWidth)
    {
        if (original.Width <= maxWidth) return original;
        int newHeight = (int)Math.Round((double)original.Height / original.Width * maxWidth);
        return original.Resize(new SKImageInfo(maxWidth, newHeight), SKFilterQuality.High)
               ?? original;
    }

    private static byte[] Encode(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data  = image.Encode(SKEncodedImageFormat.Jpeg, 85);
        return data.ToArray();
    }

    // ── Slug ─────────────────────────────────────────────────────────

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = title.ToLowerInvariant()
            .Replace(' ', '-')
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .Aggregate(string.Empty, (s, c) => s + c)
            .Trim('-');

        var slug = baseSlug;
        var suffix = 1;
        while (await _dbContext.Photos.AnyAsync(p => p.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }
        return slug;
    }

    // ── EXIF ─────────────────────────────────────────────────────────

    private static ExifData ExtractExif(byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            var dirs = ImageMetadataReader.ReadMetadata(stream);
            var ifd0   = dirs.OfType<ExifIfd0Directory>().FirstOrDefault();
            var subIfd = dirs.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            int orientation = 1;
            if (ifd0?.TryGetInt32(ExifDirectoryBase.TagOrientation, out var o) == true)
                orientation = o;

            decimal? aperture = null;
            if (subIfd?.TryGetDouble(ExifDirectoryBase.TagFNumber, out var f) == true)
                aperture = Math.Round((decimal)f, 1);

            decimal? focalLength = null;
            if (subIfd?.TryGetDouble(ExifDirectoryBase.TagFocalLength, out var fl) == true)
                focalLength = Math.Round((decimal)fl, 1);

            int? iso = null;
            if (subIfd?.TryGetInt32(ExifDirectoryBase.TagIsoEquivalent, out var isoVal) == true)
                iso = isoVal;

            string? shutter = null;
            if (subIfd?.TryGetRational(ExifDirectoryBase.TagExposureTime, out var exp) == true)
            {
                var sec = exp.ToDouble();
                shutter = sec < 1 ? $"1/{Math.Round(1 / sec)}s" : $"{sec:0.#}s";
            }

            DateTime? takenAt = null;
            if (subIfd?.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dt) == true)
                takenAt = dt;

            return new ExifData(
                takenAt,
                ifd0?.GetDescription(ExifDirectoryBase.TagMake),
                ifd0?.GetDescription(ExifDirectoryBase.TagModel),
                subIfd?.GetDescription(ExifDirectoryBase.TagLensModel),
                aperture, shutter, iso, focalLength, orientation);
        }
        catch
        {
            return new ExifData(null, null, null, null, null, null, null, null, 1);
        }
    }

    // ── EXIF orientation ─────────────────────────────────────────────

    private static SKBitmap ApplyExifOrientation(SKBitmap bitmap, int orientation) => orientation switch
    {
        2 => FlipHorizontal(bitmap),
        3 => Rotate(bitmap, 180),
        4 => FlipVertical(bitmap),
        5 => Rotate(FlipHorizontal(bitmap), 270),
        6 => Rotate(bitmap, 90),
        7 => Rotate(FlipHorizontal(bitmap), 90),
        8 => Rotate(bitmap, 270),
        _ => bitmap,
    };

    private static SKBitmap Rotate(SKBitmap bitmap, int degrees)
    {
        var swap = degrees is 90 or 270;
        var w = swap ? bitmap.Height : bitmap.Width;
        var h = swap ? bitmap.Width  : bitmap.Height;
        var rotated = new SKBitmap(w, h);
        using var canvas = new SKCanvas(rotated);
        canvas.Translate(w / 2f, h / 2f);
        canvas.RotateDegrees(degrees);
        canvas.Translate(-bitmap.Width / 2f, -bitmap.Height / 2f);
        canvas.DrawBitmap(bitmap, 0, 0);
        return rotated;
    }

    private static SKBitmap FlipHorizontal(SKBitmap bitmap)
    {
        var flipped = new SKBitmap(bitmap.Width, bitmap.Height);
        using var canvas = new SKCanvas(flipped);
        canvas.Scale(-1, 1, bitmap.Width / 2f, 0);
        canvas.DrawBitmap(bitmap, 0, 0);
        return flipped;
    }

    private static SKBitmap FlipVertical(SKBitmap bitmap)
    {
        var flipped = new SKBitmap(bitmap.Width, bitmap.Height);
        using var canvas = new SKCanvas(flipped);
        canvas.Scale(1, -1, 0, bitmap.Height / 2f);
        canvas.DrawBitmap(bitmap, 0, 0);
        return flipped;
    }

    public async Task<bool> DeleteAsync(int photoId, CancellationToken cancellationToken)
    {
        var photo = await _dbContext.Photos.FindAsync([photoId], cancellationToken);
        if (photo is null) return false;

        foreach (var path in new[] { photo.ThumbnailPath, photo.MediumPath, photo.LargePath })
        {
            try
            {
                await _storageService.DeleteVariantAsync(path, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to delete variant {Path} for photo {PhotoId}", path, photoId);
            }
        }

        _dbContext.Photos.Remove(photo);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static UploadPhotoResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };

    private record ExifData(
        DateTime? TakenAt, string? CameraMake, string? CameraModel, string? LensModel,
        decimal? Aperture, string? ShutterSpeed, int? Iso, decimal? FocalLengthMm,
        int Orientation);
}
