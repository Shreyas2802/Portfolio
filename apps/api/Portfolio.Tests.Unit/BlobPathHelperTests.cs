using Xunit;
using Portfolio.Infrastructure.Photography;

namespace Portfolio.Tests.Unit;

public class BlobPathHelperTests
{
    private const string BaseUrl = "https://example.blob.core.windows.net/photos";

    [Theory]
    [InlineData("photos/sunset/large.jpg", "photos/sunset/large.jpg")]
    [InlineData("photos\\sunset\\large.jpg", "photos/sunset/large.jpg")]
    [InlineData("/photos/sunset/large.jpg/", "photos/sunset/large.jpg")]
    public void ToBlobName_NormalisesSeparators(string input, string expected)
    {
        Assert.Equal(expected, BlobPathHelper.ToBlobName(input));
    }

    [Theory]
    [InlineData("photos/../secrets.txt")]
    [InlineData("../photos/large.jpg")]
    [InlineData("photos/./large.jpg")]
    [InlineData("photos//large.jpg")]
    public void ToBlobName_RejectsTraversalAndEmptySegments(string input)
    {
        Assert.Throws<InvalidOperationException>(() => BlobPathHelper.ToBlobName(input));
    }

    [Fact]
    public void PublicUrl_RoundTripsToBlobName()
    {
        var url = BlobPathHelper.ToPublicUrl(BaseUrl + "/", "photos/golden path/large.jpg");

        Assert.Equal(BaseUrl + "/photos/golden%20path/large.jpg", url);
        Assert.True(BlobPathHelper.TryGetBlobName(url, [BaseUrl], out var blobName));
        Assert.Equal("photos/golden path/large.jpg", blobName);
    }

    [Fact]
    public void TryGetBlobName_AcceptsAnyKnownBaseUrl()
    {
        var url = "https://cdn.example.com/photos/sunset/thumb.jpg";

        Assert.True(BlobPathHelper.TryGetBlobName(url, [BaseUrl, "https://cdn.example.com"], out var blobName));
        Assert.Equal("photos/sunset/thumb.jpg", blobName);
    }

    [Theory]
    [InlineData("/uploads/photos/sunset/large.jpg")]
    [InlineData("https://other.blob.core.windows.net/photos/photos/sunset/large.jpg")]
    [InlineData("https://example.blob.core.windows.net/photos/photos/%2E%2E/large.jpg")]
    [InlineData("")]
    public void TryGetBlobName_RejectsForeignOrUnsafeUrls(string url)
    {
        Assert.False(BlobPathHelper.TryGetBlobName(url, [BaseUrl], out _));
    }

    [Fact]
    public void TryGetBlobName_IgnoresQueryString()
    {
        Assert.True(BlobPathHelper.TryGetBlobName(BaseUrl + "/photos/a/large.jpg?v=2", [BaseUrl], out var blobName));
        Assert.Equal("photos/a/large.jpg", blobName);
    }

    [Theory]
    [InlineData("/uploads/photos/a/large.jpg", true)]
    [InlineData("https://example.blob.core.windows.net/photos/a/large.jpg", false)]
    public void IsLocalPath_DetectsLegacyDiskPaths(string url, bool expected)
    {
        Assert.Equal(expected, BlobPathHelper.IsLocalPath(url));
    }

    [Fact]
    public void TryGetPublicBaseUrl_UsesAzuriteContainerUrl()
    {
        var url = AzureBlobPhotoStorageService.TryGetPublicBaseUrl(new AzureBlobStorageOptions
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ContainerName = "photos",
        });

        Assert.Equal("http://127.0.0.1:10000/devstoreaccount1/photos", url);
    }

    [Fact]
    public void TryGetPublicBaseUrl_PrefersConfiguredCdnUrl()
    {
        var url = AzureBlobPhotoStorageService.TryGetPublicBaseUrl(new AzureBlobStorageOptions
        {
            ConnectionString = "UseDevelopmentStorage=true",
            PublicBaseUrl = "https://cdn.example.com/",
        });

        Assert.Equal("https://cdn.example.com", url);
    }

    [Fact]
    public void TryGetPublicBaseUrl_ReturnsNullWithoutConnectionString()
    {
        Assert.Null(AzureBlobPhotoStorageService.TryGetPublicBaseUrl(new AzureBlobStorageOptions()));
    }
}
