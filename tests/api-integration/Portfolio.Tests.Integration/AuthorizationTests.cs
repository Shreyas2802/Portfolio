using System.Net;
using Xunit;

namespace Portfolio.Tests.Integration;

/// <summary>
/// Covers the spec's explicit E2E case "Unauthorized visitor cannot
/// access admin API" at the integration level, across every admin-only
/// endpoint - not just one example.
/// </summary>
public class AuthorizationTests : IClassFixture<PortfolioApiFactory>
{
    private readonly PortfolioApiFactory _factory;

    public AuthorizationTests(PortfolioApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("GET", "/api/photography/photos")]
    [InlineData("GET", "/api/photography/albums/admin/all")]
    [InlineData("POST", "/api/photography/albums")]
    [InlineData("POST", "/api/photography/upload")]
    [InlineData("GET", "/api/projects/admin/all")]
    [InlineData("POST", "/api/projects")]
    [InlineData("GET", "/api/admin/stats")]
    public async Task AdminEndpoint_WithoutToken_ReturnsUnauthorized(string method, string path)
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), path);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithValidAdminToken_Succeeds()
    {
        // The positive case, so a bug that broke auth entirely (e.g.
        // rejecting every token, valid or not) wouldn't slip past the
        // negative tests above looking like success.
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.GetAsync("/api/admin/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
