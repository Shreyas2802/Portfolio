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
        using var original = SKBitmap.Decode(bytes);
        if (original is null)
        {
            return Fail("File is not a valid, decodable image.");
        }

        var exif = ExtractExif(bytes);
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
        decimal? Aperture, string? ShutterSpeed, int? Iso, decimal? FocalLengthMm);

    /// <summary>
    /// Reads real EXIF from the original file bytes via MetadataExtractor.
    /// Returns an all-null result rather than throwing if a photo has no
    /// EXIF (e.g. a screenshot, or a scan) - camera fields are nullable
    /// on Photo for exactly this reason.
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

            return new ExifData(takenAt, make, model, lens, aperture, shutter, iso, focalLength);
        }
        catch
        {
            // No EXIF, or unparseable - a photo without camera data is
            // still a valid upload, it just won't show a DataLabel.
            return new ExifData(null, null, null, null, null, null, null, null);
        }
    }
}
