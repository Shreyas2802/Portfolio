using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Portfolio.Tests.Integration;

public class ContactFormTests : IClassFixture<PortfolioApiFactory>
{
    private readonly PortfolioApiFactory _factory;

    public ContactFormTests(PortfolioApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Submit_ValidMessage_ReturnsOkAndPersistsToDatabase()
    {
        var client = _factory.CreateClient();
        var uniqueEmail = $"integration-test-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/contact", new
        {
            name = "Integration Test",
            email = uniqueEmail,
            message = "This is a real contact form submission from an integration test.",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var db = _factory.CreateDbContext();
        var stored = await db.ContactMessages.FirstOrDefaultAsync(m => m.Email == uniqueEmail);
        Assert.NotNull(stored);
        Assert.Equal("Integration Test", stored!.Name);
    }

    [Fact]
    public async Task Submit_WithHoneypotFilled_ReturnsOkButDropsMessageSilently()
    {
        // Matches the Phase 3 design exactly: always return success (so
        // a bot never learns its submission was rejected) but the
        // message must never actually reach the database.
        var client = _factory.CreateClient();
        var uniqueEmail = $"integration-test-honeypot-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/contact", new
        {
            name = "Bot",
            email = uniqueEmail,
            message = "This should be silently dropped.",
            website = "http://spam-bot-filled-this-in.example",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var db = _factory.CreateDbContext();
        var stored = await db.ContactMessages.FirstOrDefaultAsync(m => m.Email == uniqueEmail);
        Assert.Null(stored);
    }

    [Theory]
    [InlineData("", "valid@example.com", "A real message")] // missing name
    [InlineData("Name", "not-an-email", "A real message")] // invalid email
    [InlineData("Name", "valid@example.com", "")] // missing message
    public async Task Submit_InvalidInput_ReturnsBadRequest(string name, string email, string message)
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/contact", new { name, email, message });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
