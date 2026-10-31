using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Portfolio.Infrastructure.Photography;

/// <summary>
/// Startup checks for the Blob provider: fail fast on missing settings
/// (a clear error at boot beats a 500 on every upload), create the
/// container, and apply CORS rules for the configured origins.
/// </summary>
public static class AzureBlobStorageInitializer
{
    public static void Validate(AzureBlobStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException(
                "Storage:Provider is AzureBlob but Storage:AzureBlob:ConnectionString is not set. " +
                "For local development use \"UseDevelopmentStorage=true\" with Azurite running.");
        }

        if (string.IsNullOrWhiteSpace(options.ContainerName))
        {
            throw new InvalidOperationException("Storage:AzureBlob:ContainerName must not be empty.");
        }
    }

    public static async Task InitializeAsync(
        BlobServiceClient serviceClient,
        AzureBlobStorageOptions options,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var container = serviceClient.GetBlobContainerClient(options.ContainerName);

        try
        {
            // Public read access for individual blobs (not listing), so the
            // browser can load images directly without going through the API.
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PublicAccessNotPermitted")
        {
            // New Azure storage accounts disable anonymous access by default.
            await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
            logger.LogError(
                "Blob container {Container} was created as private because the storage account does not allow " +
                "anonymous blob access. Photos will not load in the browser until you either enable " +
                "'Allow blob anonymous access' on the account and set the container's access level to Blob, " +
                "or set Storage:AzureBlob:PublicBaseUrl to a CDN that can read it.",
                options.ContainerName);
        }

        if (options.CorsAllowedOrigins.Length > 0)
        {
            await ApplyCorsAsync(serviceClient, options.CorsAllowedOrigins, cancellationToken);
        }

        logger.LogInformation("Photo storage: Azure Blob container {Container} at {Url}",
            options.ContainerName, container.Uri);
    }

    private static async Task ApplyCorsAsync(
        BlobServiceClient serviceClient, string[] origins, CancellationToken cancellationToken)
    {
        BlobServiceProperties properties = await serviceClient.GetPropertiesAsync(cancellationToken);

        var allowedOrigins = string.Join(',', origins.Select(o => o.TrimEnd('/')));

        // Replace only our own read-only rule; keep any rules added by hand.
        var rules = (properties.Cors ?? new List<BlobCorsRule>())
            .Where(r => !(r.AllowedMethods == "GET,HEAD" && r.AllowedHeaders == "*"))
            .ToList();
        rules.Add(new BlobCorsRule
        {
            AllowedOrigins = allowedOrigins,
            AllowedMethods = "GET,HEAD",
            AllowedHeaders = "*",
            ExposedHeaders = "*",
            MaxAgeInSeconds = 3600,
        });

        properties.Cors = rules;
        await serviceClient.SetPropertiesAsync(properties, cancellationToken);
    }
}
