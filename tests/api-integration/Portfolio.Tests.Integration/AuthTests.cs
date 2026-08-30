using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Portfolio.Tests.Integration;

public class AuthTests : IClassFixture<PortfolioApiFactory>
{
    private readonly PortfolioApiFactory _factory;

    public AuthTests(PortfolioApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidAdminCredentials_ReturnsTokenAndOk()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = PortfolioApiFactory.TestAdminUsername,
            password = PortfolioApiFactory.TestAdminPassword,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.False(string.IsNullOrEmpty(body.GetProperty("token").GetString()));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = PortfolioApiFactory.TestAdminUsername,
            password = "definitely-the-wrong-password",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "no-such-user-exists",
            password = "whatever",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownUsernameAndWrongPassword_ReturnIdenticalErrorBody()
    {
        // Regression test for the Phase 6a decision: the response must
        // never let a caller distinguish "no such user" from "wrong
        // password" - that distinction is exactly what lets an attacker
        // enumerate valid usernames one guess at a time.
        var client = _factory.CreateClient();

        var unknownUserResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "no-such-user-exists",
            password = "whatever",
        });
        var wrongPasswordResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = PortfolioApiFactory.TestAdminUsername,
            password = "wrong-password",
        });

        var unknownUserBody = await unknownUserResponse.Content.ReadAsStringAsync();
        var wrongPasswordBody = await wrongPasswordResponse.Content.ReadAsStringAsync();

        Assert.Equal(unknownUserResponse.StatusCode, wrongPasswordResponse.StatusCode);
        Assert.Equal(unknownUserBody, wrongPasswordBody);
    }
}
