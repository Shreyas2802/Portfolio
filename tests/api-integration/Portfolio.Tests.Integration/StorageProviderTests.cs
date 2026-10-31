using System.Net;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Portfolio.Application.Photography;
using Portfolio.Infrastructure.Photography;
using Xunit;

namespace Portfolio.Tests.Integration;

public class StorageProviderTests : IClassFixture<PortfolioApiFactory>
{
    private readonly PortfolioApiFactory _factory;

    public StorageProviderTests(PortfolioApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void DefaultProvider_IsLocalDisk()
    {
        using var scope = _factory.Services.CreateScope();

        var storage = scope.ServiceProvider.GetRequiredService<IPhotoStorageService>();

        Assert.IsType<LocalPhotoStorageService>(storage);
    }

    [Fact]
    public async Task LocalProvider_KeepsImgSrcLockedToSelf()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        var csp = string.Join(';', response.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("img-src 'self' data:;", csp);
    }
}

/// <summary>
/// Round-trips real blobs through Azurite (the Azure Storage emulator).
/// CI starts Azurite and sets AZURITE_CONNECTION_STRING; without it
/// (e.g. a quick local run) the test returns early instead of failing.
/// </summary>
public class AzureBlobStorageTests
{
    private const string SiteOrigin = "https://portfolio.vistrut.com";

    [Fact]
    public async Task SaveServeAndDelete_RoundTripsThroughBlobStorage()
    {
        var connectionString = Environment.GetEnvironmentVariable("AZURITE_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString)) return;

        var blobOptions = new AzureBlobStorageOptions
        {
            ConnectionString = connectionString,
            ContainerName = "test-" + Guid.NewGuid().ToString("N")[..12],
            CorsAllowedOrigins = [SiteOrigin],
        };
        var serviceClient = new BlobServiceClient(connectionString);
        var container = serviceClient.GetBlobContainerClient(blobOptions.ContainerName);
        var localRoot = Path.Combine(Path.GetTempPath(), "portfolio-storage-test-" + Guid.NewGuid().ToString("N"));

        try
        {
            await AzureBlobStorageInitializer.InitializeAsync(serviceClient, blobOptions, NullLogger.Instance);

            var storageOptions = Options.Create(new StorageOptions
            {
                Provider = StorageProvider.AzureBlob,
                UploadsRootPath = localRoot,
                AzureBlob = blobOptions,
            });
            var localStorage = new LocalPhotoStorageService(
                new HostingEnvironment { ContentRootPath = localRoot }, storageOptions);
            var storage = new AzureBlobPhotoStorageService(
                container, localStorage, storageOptions, NullLogger<AzureBlobPhotoStorageService>.Instance);

            var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3, 4, 0xFF, 0xD9 };

            // Save: returns a public blob URL, not a local /uploads path.
            var url = await storage.SaveVariantAsync("photos/test-slug/thumb.jpg", bytes, CancellationToken.None);
            Assert.Equal(container.Uri.ToString().TrimEnd('/') + "/photos/test-slug/thumb.jpg", url);

            // Serve: an anonymous browser-style GET works, with the right headers.
            using var http = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Origin", SiteOrigin);
            using var response = await http.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/jpeg", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("immutable", response.Headers.CacheControl?.ToString());
            Assert.Equal(SiteOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
            Assert.Equal(bytes, await response.Content.ReadAsByteArrayAsync());

            // Delete: the blob is gone afterwards.
            await storage.DeleteVariantAsync(url, CancellationToken.None);
            using var afterDelete = await http.GetAsync(url);
            Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);

            // Legacy photos still on disk are deleted through the local provider.
            var legacyPath = await localStorage.SaveVariantAsync("photos/old-photo/thumb.jpg", bytes, CancellationToken.None);
            Assert.StartsWith("/uploads/", legacyPath);
            await storage.DeleteVariantAsync(legacyPath, CancellationToken.None);
            Assert.False(File.Exists(Path.Combine(localRoot, "photos", "old-photo", "thumb.jpg")));
        }
        finally
        {
            await container.DeleteIfExistsAsync();
            if (Directory.Exists(localRoot)) Directory.Delete(localRoot, recursive: true);
        }
    }
}
