using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portfolio.Application.Photography;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;
using SkiaSharp;

namespace Portfolio.Infrastructure.Photography;

/// <summary>
/// Handles the full upload pipeline (spec Phase 5): validate the file
/// is genuinely a decodable image, extract real EXIF, generate the
/// three size variants, persist them via the storage abstraction, and
/// create the Photo row - or clean up anything already written if a
/// later step fails, so a failed upload never leaves orphaned files
/// or a half-written database row.
/// </summary>
public class PhotoUploadService : IPhotoUploadService
{
    private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB
    private static readonly (string Name, int MaxWidth)[] Variants =
    {
        ("thumb", 480),
        ("medium", 1200),
        ("large", 2400),
    };

    private readonly PortfolioDbContext _dbContext;
    private readonly IPhotoStorageService _storageService;
    private readonly ILogger<PhotoUploadService> _logger;

    public PhotoUploadService(
        PortfolioDbContext dbContext,
        IPhotoStorageService storageService,
        ILogger<PhotoUploadService> logger)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<UploadPhotoResult> UploadAsync(UploadPhotoRequest request, CancellationToken cancellationToken)
    {
        var album = await _dbContext.Albums.FindAsync([request.AlbumId], cancellationToken);
        if (album is null)
        {
            return Fail("Album not found.");
        }

        using var memoryStream = new MemoryStream();
        await request.FileStream.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        if (bytes.Length == 0)
        {
            return Fail("Uploaded file is empty.");
        }
        if (bytes.Length > MaxFileSizeBytes)
        {
            return Fail($"File exceeds the {MaxFileSizeBytes / (1024 * 1024)}MB limit.");
        }

        // Never trust the declared content-type - decode the actual
        // bytes. If SkiaSharp can't decode it, it isn't a real image,
        // regardless of what extension or content-type it arrived with.
        var decoded = SKBitmap.Decode(bytes);
        if (decoded is null)
        {
            return Fail(
                "Couldn't read this file as an image. Supported formats: JPEG, PNG, WEBP. " +
                "HEIC (common on iPhone) and RAW formats aren't supported - convert to JPEG first.");
        }

        try
        {
            var exif = ExtractExif(bytes);

            // Apply the EXIF-declared rotation/flip to the actual pixels
            // now, once, rather than per-variant - see ApplyExifOrientation
            // for why this is necessary at all.
            var oriented = ApplyExifOrientation(decoded, exif.Orientation);
            try
            {
                return await SaveUploadAsync(request, album, oriented, exif, cancellationToken);
            }
            finally
            {
                if (!ReferenceEquals(oriented, decoded))
                {
                    oriented.Dispose();
                }
            }
        }
        finally
        {
            decoded.Dispose();
        }
    }

    private async Task<UploadPhotoResult> SaveUploadAsync(
        UploadPhotoRequest request, Album album, SKBitmap original, ExifData exif, CancellationToken cancellationToken)
    {
        var slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken);
        var savedPaths = new List<string>();

        try
        {
            var paths = new Dictionary<string, string>();
            foreach (var (name, maxWidth) in Variants)
            {
                var variantBytes = ResizeAndEncode(original, maxWidth);
                var relativePath = $"photos/{slug}/{name}.jpg";
                var urlPath = await _storageService.SaveVariantAsync(relativePath, variantBytes, cancellationToken);
                savedPaths.Add(urlPath);
                paths[name] = urlPath;
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
                DisplayOrder = await _dbContext.Photos.CountAsync(p => p.AlbumId == album.Id, cancellationToken),
                IsPublished = true,
                UploadedAtUtc = DateTime.UtcNow,
            };

            if (request.TagIds.Count > 0)
            {
                var tags = await _dbContext.Tags
                    .Where(t => request.TagIds.Contains(t.Id))
                    .ToListAsync(cancellationToken);
                foreach (var tag in tags)
                {
                    photo.Tags.Add(tag);
                }
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
            _logger.LogError(ex, "Photo upload failed after writing {Count} variant(s); cleaning up", savedPaths.Count);
            foreach (var path in savedPaths)
            {
                try
                {
                    await _storageService.DeleteVariantAsync(path, cancellationToken);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Failed to clean up orphaned variant {Path}", path);
                }
            }
            return Fail("Upload failed while saving the photo. No files were left behind.");
        }
    }

    public async Task<bool> DeleteAsync(int photoId, CancellationToken cancellationToken)
    {
        var photo = await _dbContext.Photos.FindAsync([photoId], cancellationToken);
        if (photo is null)
        {
            return false;
        }

        foreach (var path in new[] { photo.ThumbnailPath, photo.MediumPath, photo.LargePath })
        {
            try
            {
                await _storageService.DeleteVariantAsync(path, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log and continue - a missing/already-deleted file
                // shouldn't block removing the database row, which is
                // the part the admin actually cares about here.
                _logger.LogWarning(ex, "Failed to delete variant {Path} for photo {PhotoId}", path, photoId);
            }
        }

        _dbContext.Photos.Remove(photo);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static byte[] ResizeAndEncode(SKBitmap original, int maxWidth)
    {
        SKBitmap toEncode = original;
        SKBitmap? resized = null;
        if (original.Width > maxWidth)
        {
            var ratio = (float)maxWidth / original.Width;
            var newHeight = (int)Math.Round(original.Height * ratio);
            resized = original.Resize(new SKImageInfo(maxWidth, newHeight), SKFilterQuality.High);
            toEncode = resized ?? original;
        }

        using var image = SKImage.FromBitmap(toEncode);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);
        resized?.Dispose();
        return data.ToArray();
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.Slugify(title);
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "photo";
        }

        var slug = baseSlug;
        var suffix = 1;
        while (await _dbContext.Photos.AnyAsync(p => p.Slug == slug, cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }
        return slug;
    }

    private static UploadPhotoResult Fail(string message) => new() { Success = false, ErrorMessage = message };

    private record ExifData(
        DateTime? TakenAt, string? CameraMake, string? CameraModel, string? LensModel,
        decimal? Aperture, string? ShutterSpeed, int? Iso, decimal? FocalLengthMm, int Orientation);

    /// <summary>
    /// Reads real EXIF from the original file bytes via MetadataExtractor.
    /// Returns an all-null result rather than throwing if a photo has no
    /// EXIF (e.g. a screenshot, or a scan) - camera fields are nullable
    /// on Photo for exactly this reason. Orientation defaults to 1
    /// (normal) when absent or unparseable.
    /// </summary>
    private static ExifData ExtractExif(byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            var directories = ImageMetadataReader.ReadMetadata(stream);

            var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            string? make = ifd0?.GetDescription(ExifDirectoryBase.TagMake);
            string? model = ifd0?.GetDescription(ExifDirectoryBase.TagModel);
            string? lens = subIfd?.GetDescription(ExifDirectoryBase.TagLensModel);

            int orientation = 1;
            if (ifd0?.TryGetInt32(ExifDirectoryBase.TagOrientation, out var orientationValue) == true)
            {
                orientation = orientationValue;
            }

            decimal? aperture = null;
            if (subIfd?.TryGetDouble(ExifDirectoryBase.TagFNumber, out var fNumber) == true)
            {
                aperture = Math.Round((decimal)fNumber, 1);
            }

            decimal? focalLength = null;
            if (subIfd?.TryGetDouble(ExifDirectoryBase.TagFocalLength, out var focal) == true)
            {
                focalLength = Math.Round((decimal)focal, 1);
            }

            int? iso = null;
            if (subIfd?.TryGetInt32(ExifDirectoryBase.TagIsoEquivalent, out var isoValue) == true)
            {
                iso = isoValue;
            }

            string? shutter = null;
            if (subIfd?.TryGetRational(ExifDirectoryBase.TagExposureTime, out var exposure) == true)
            {
                var seconds = exposure.ToDouble();
                shutter = seconds < 1
                    ? $"1/{Math.Round(1 / seconds)}s"
                    : $"{seconds:0.#}s";
            }

            DateTime? takenAt = null;
            if (subIfd?.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTime) == true)
            {
                takenAt = dateTime;
            }

            return new ExifData(takenAt, make, model, lens, aperture, shutter, iso, focalLength, orientation);
        }
        catch
        {
            // No EXIF, or unparseable - a photo without camera data is
            // still a valid upload, it just won't show a DataLabel.
            return new ExifData(null, null, null, null, null, null, null, null, 1);
        }
    }

    /// <summary>
    /// Cameras commonly store sensor pixel data in a fixed landscape
    /// orientation and rely on the EXIF Orientation tag to tell viewers
    /// how to rotate/flip it for display - SkiaSharp's decode does NOT
    /// apply this automatically, it just hands back the raw pixels. Left
    /// unhandled, a portrait photo shot on a camera that stores raw
    /// landscape data (e.g. this site's Sony A6000) gets saved and
    /// displayed sideways. This physically rotates/flips the pixels so
    /// the saved variants need no orientation tag at all - consistent
    /// with already stripping EXIF from served files (see Phase 4).
    /// </summary>
    private static SKBitmap ApplyExifOrientation(SKBitmap bitmap, int orientation) => orientation switch
    {
        2 => FlipHorizontal(bitmap),
        3 => Rotate(bitmap, 180),
        4 => FlipVertical(bitmap),
        5 => Rotate(FlipHorizontal(bitmap), 270),
        6 => Rotate(bitmap, 90),
        7 => Rotate(FlipHorizontal(bitmap), 90),
        8 => Rotate(bitmap, 270),
        _ => bitmap, // 1, or missing/unrecognized - already correct
    };

    private static SKBitmap Rotate(SKBitmap bitmap, int degrees)
    {
        var swapDimensions = degrees is 90 or 270;
        var width = swapDimensions ? bitmap.Height : bitmap.Width;
        var height = swapDimensions ? bitmap.Width : bitmap.Height;

        var rotated = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(rotated))
        {
            // Rotate around the new canvas's center, then shift back by
            // the original bitmap's center before drawing - this is the
            // standard "rotate around center" composition and avoids
            // hand-deriving translate offsets per orientation case.
            canvas.Translate(width / 2f, height / 2f);
            canvas.RotateDegrees(degrees);
            canvas.Translate(-bitmap.Width / 2f, -bitmap.Height / 2f);
            canvas.DrawBitmap(bitmap, 0, 0);
        }
        return rotated;
    }

    private static SKBitmap FlipHorizontal(SKBitmap bitmap)
    {
        var flipped = new SKBitmap(bitmap.Width, bitmap.Height);
        using (var canvas = new SKCanvas(flipped))
        {
            canvas.Scale(-1, 1, bitmap.Width / 2f, 0);
            canvas.DrawBitmap(bitmap, 0, 0);
        }
        return flipped;
    }

    private static SKBitmap FlipVertical(SKBitmap bitmap)
    {
        var flipped = new SKBitmap(bitmap.Width, bitmap.Height);
        using (var canvas = new SKCanvas(flipped))
        {
            canvas.Scale(1, -1, 0, bitmap.Height / 2f);
            canvas.DrawBitmap(bitmap, 0, 0);
        }
        return flipped;
    }
}
