using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Photography;

namespace Portfolio.Infrastructure.Photography;

/// <summary>
/// Azure Blob Storage implementation of the storage abstraction - the
/// upload service and controllers are unchanged, they only see
/// IPhotoStorageService.
///
/// Blob names mirror the local layout ("photos/{slug}/large.jpg"), so
/// moving existing files from disk to Blob is a plain folder copy.
///
/// Photos uploaded before the switch keep their "/uploads/..." paths in
/// the database and are still served from disk; deletes for those are
/// handed to the local provider, so switching providers needs no data
/// migration up front.
/// </summary>
public class AzureBlobPhotoStorageService : IPhotoStorageService
{
    // Variants are never modified in place (re-upload creates a new slug),
    // so browsers and CDNs can cache them for a year.
    private const string ImmutableCacheControl = "public, max-age=31536000, immutable";

    private readonly BlobContainerClient _container;
    private readonly LocalPhotoStorageService _localStorage;
    private readonly ILogger<AzureBlobPhotoStorageService> _logger;
    private readonly string _publicBaseUrl;
    private readonly string[] _knownBaseUrls;

    public AzureBlobPhotoStorageService(
        BlobContainerClient container,
        LocalPhotoStorageService localStorage,
        IOptions<StorageOptions> storageOptions,
        ILogger<AzureBlobPhotoStorageService> logger)
    {
        _container = container;
        _localStorage = localStorage;
        _logger = logger;

        var containerUrl = container.Uri.ToString().TrimEnd('/');
        var configuredBase = storageOptions.Value.AzureBlob.PublicBaseUrl;
        _publicBaseUrl = string.IsNullOrWhiteSpace(configuredBase) ? containerUrl : configuredBase.TrimEnd('/');

        // Accept both, so changing PublicBaseUrl (e.g. adding a CDN later)
        // doesn't stop older photos from being deleted.
        _knownBaseUrls = [_publicBaseUrl, containerUrl];
    }

    public async Task<string> SaveVariantAsync(string relativePath, byte[] content, CancellationToken cancellationToken)
    {
        var blobName = BlobPathHelper.ToBlobName(relativePath);
        var blob = _container.GetBlobClient(blobName);

        await blob.UploadAsync(
            BinaryData.FromBytes(content),
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "image/jpeg",
                    CacheControl = ImmutableCacheControl,
                },
            },
            cancellationToken);

        return BlobPathHelper.ToPublicUrl(_publicBaseUrl, blobName);
    }

    public async Task DeleteVariantAsync(string urlPath, CancellationToken cancellationToken)
    {
        if (BlobPathHelper.IsLocalPath(urlPath))
        {
            await _localStorage.DeleteVariantAsync(urlPath, cancellationToken);
            return;
        }

        if (!BlobPathHelper.TryGetBlobName(urlPath, _knownBaseUrls, out var blobName))
        {
            _logger.LogWarning("Not deleting {Url}: it does not belong to this blob container", urlPath);
            return;
        }

        await _container.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// The public base URL photos will be served from, without touching
    /// the network - used to add the right origin to the CSP img-src.
    /// Returns null if Blob storage isn't usable with these options.
    /// </summary>
    public static string? TryGetPublicBaseUrl(AzureBlobStorageOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.PublicBaseUrl))
        {
            return options.PublicBaseUrl.TrimEnd('/');
        }

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return null;
        }

        try
        {
            return new BlobContainerClient(options.ConnectionString, options.ContainerName).Uri.ToString().TrimEnd('/');
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or InvalidOperationException)
        {
            return null;
        }
    }
}
