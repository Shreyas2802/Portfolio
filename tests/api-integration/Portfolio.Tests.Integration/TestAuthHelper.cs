using System.Net.Http.Json;
using System.Text.Json;

namespace Portfolio.Tests.Integration;

internal static class TestAuthHelper
{
    /// <summary>
    /// Logs in through the real /api/auth/login endpoint (not a
    /// hand-crafted token) and returns the JWT - so tests that need an
    /// authenticated client are exercising the actual login flow, not
    /// bypassing it.
    /// </summary>
    public static async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = PortfolioApiFactory.TestAdminUsername,
            password = PortfolioApiFactory.TestAdminPassword,
        });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("token").GetString()!;
    }

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(PortfolioApiFactory factory)
    {
        var client = factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
