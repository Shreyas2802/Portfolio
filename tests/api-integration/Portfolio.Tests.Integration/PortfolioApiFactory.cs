using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Tests.Integration;

/// <summary>
/// Boots the real ASP.NET Core app (real Program.cs, real middleware
/// pipeline, real controllers) against an in-memory SQLite database
/// instead of SQL Server, so integration tests run without needing a
/// real SQL Server/LocalDB instance available. Runs under a distinct
/// "Testing" environment name (not "Development") specifically so the
/// dev-only Photo/Project seeders (DbSeeder, ProjectSeeder) do NOT
/// run - each test seeds exactly the data it needs directly via the
/// DbContext, for deterministic, test-local fixtures. The Identity
/// admin-user seeder still runs (it's unconditional by design, see
/// Phase 6a) using test-only credentials configured below - never
/// real secrets.
/// </summary>
public class PortfolioApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public const string TestAdminUsername = "integration-test-admin";
    public const string TestAdminPassword = "Integration-Test-Password-123!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "integration-test-signing-key-at-least-32-characters",
                ["Jwt:Issuer"] = "PortfolioApi.Tests",
                ["Jwt:Audience"] = "PortfolioClient.Tests",
                ["AdminUser:Username"] = TestAdminUsername,
                ["AdminUser:Password"] = TestAdminPassword,
            });
        });

        builder.ConfigureServices(services =>
        {
            // AddDbContext registers TWO separate things: a
            // DbContextOptions<T> singleton, AND a distinct
            // IDbContextOptionsConfiguration<T> delegate that gets
            // *replayed* whenever the options are actually built.
            // Removing only the former (as an earlier version of this
            // file did) left the latter in place - so when the SQLite
            // registration below ran, EF Core replayed BOTH the
            // original UseSqlServer configuration (from Program.cs)
            // AND this UseSqlite configuration onto the same options
            // object, producing "two database providers registered".
            // RemoveAll clears every registration of a given service
            // type in one call, and is also safer than
            // SingleOrDefault()+Remove() here, which would throw if
            // more than one matching descriptor ever existed.
            services.RemoveAll(typeof(DbContextOptions<PortfolioDbContext>));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<PortfolioDbContext>));

            _connection.Open();
            services.AddDbContext<PortfolioDbContext>(options => options.UseSqlite(_connection));

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
            db.Database.EnsureCreated();
        });
    }

    /// <summary>Convenience for tests: a fresh scoped DbContext to seed or assert against directly.</summary>
    public PortfolioDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
