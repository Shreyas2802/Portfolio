using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Xunit;

namespace Portfolio.Tests.Integration;

/// <summary>
/// Covers the spec's explicit "public/private content separation" case:
/// an unpublished album (or a photo within it) must never be reachable
/// through any public endpoint, regardless of how directly it's
/// requested - not just absent from listings, but a real 404 if
/// someone guesses or bookmarks the exact URL.
/// </summary>
public class PublicPrivateSeparationTests : IClassFixture<PortfolioApiFactory>
{
    private readonly PortfolioApiFactory _factory;

    public PublicPrivateSeparationTests(PortfolioApiFactory factory)
    {
        _factory = factory;
    }

    private async Task SeedAlbumsAsync()
    {
        await using var db = _factory.CreateDbContext();

        // Idempotent - IClassFixture shares the factory (and its
        // in-memory database) across every test in this class, so a
        // second test seeding the same slugs would otherwise collide.
        if (await db.Albums.AnyAsync(a => a.Slug == "published-test-album"))
        {
            return;
        }

        db.Albums.AddRange(
            new Album
            {
                Title = "Published Test Album",
                Slug = "published-test-album",
                Description = "Visible to the public.",
                IsPublished = true,
                DisplayOrder = 0,
                CreatedAtUtc = DateTime.UtcNow,
            },
            new Album
            {
                Title = "Unpublished Test Album",
                Slug = "unpublished-test-album",
                Description = "Should never be reachable publicly.",
                IsPublished = false,
                DisplayOrder = 1,
                CreatedAtUtc = DateTime.UtcNow,
            });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAlbums_OnlyReturnsPublishedAlbums()
    {
        await SeedAlbumsAsync();
        var client = _factory.CreateClient();

        var albums = await client.GetFromJsonAsync<List<System.Text.Json.JsonElement>>("/api/photography/albums");

        Assert.Contains(albums!, a => a.GetProperty("slug").GetString() == "published-test-album");
        Assert.DoesNotContain(albums!, a => a.GetProperty("slug").GetString() == "unpublished-test-album");
    }

    [Fact]
    public async Task GetAlbumBySlug_UnpublishedAlbum_Returns404NotSomethingElse()
    {
        await SeedAlbumsAsync();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/photography/albums/unpublished-test-album");

        // Specifically 404, not 401/403 - an unpublished album should
        // look indistinguishable from one that doesn't exist at all to
        // an unauthenticated caller, not reveal "this exists but you
        // can't see it."
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAlbumPhotos_UnpublishedAlbum_Returns404()
    {
        await SeedAlbumsAsync();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/photography/albums/unpublished-test-album/photos?page=1&pageSize=24");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
