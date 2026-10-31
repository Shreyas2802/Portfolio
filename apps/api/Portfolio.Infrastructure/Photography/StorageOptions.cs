namespace Portfolio.Infrastructure.Photography;

/// <summary>Where uploaded photo variants are stored.</summary>
public enum StorageProvider
{
    /// <summary>Local disk (wwwroot/uploads or Storage:UploadsRootPath). Default.</summary>
    Local,

    /// <summary>Azure Blob Storage (or the Azurite emulator in development).</summary>
    AzureBlob,
}

/// <summary>
/// Bound from the "Storage" config section.
///
/// Provider defaults to Local, so existing deployments keep their
/// current behaviour until Storage:Provider is set to AzureBlob.
///
/// UploadsRootPath (Local only) is optional - when unset, storage falls
/// back to wwwroot/uploads. In production it should point OUTSIDE the
/// deployed app's publish folder, because republishing replaces that
/// folder's contents and would delete every uploaded photo inside it.
/// </summary>
public class StorageOptions
{
    public StorageProvider Provider { get; set; } = StorageProvider.Local;

    public string? UploadsRootPath { get; set; }

    public AzureBlobStorageOptions AzureBlob { get; set; } = new();
}

/// <summary>Bound from "Storage:AzureBlob". Only used when Provider is AzureBlob.</summary>
public class AzureBlobStorageOptions
{
    /// <summary>
    /// Storage account connection string. "UseDevelopmentStorage=true"
    /// targets the local Azurite emulator. Keep the real value in user
    /// secrets / environment variables - never in appsettings.json.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>Container that holds the photo variants. Created on startup if missing.</summary>
    public string ContainerName { get; set; } = "photos";

    /// <summary>
    /// Optional public base URL to put in front of blob names instead of
    /// the container URL - e.g. an Azure CDN / Front Door endpoint.
    /// </summary>
    public string? PublicBaseUrl { get; set; }

    /// <summary>
    /// Origins allowed to read blobs cross-origin (CORS). Needed for the
    /// admin's hero-palette extraction, which draws the image onto a
    /// canvas. Applied to the storage account on startup when set.
    /// </summary>
    public string[] CorsAllowedOrigins { get; set; } = [];
}
