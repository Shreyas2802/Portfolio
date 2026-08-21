using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Portfolio.Infrastructure.Identity;

/// <summary>
/// This is a single-owner personal site, not a multi-tenant SaaS - so
/// there's deliberately no public registration flow. Instead, the one
/// admin account is created from configuration on startup if it
/// doesn't already exist. Safe to leave enabled in any environment:
/// it's idempotent (does nothing once the user exists) and does
/// nothing at all if AdminUser:Username/Password aren't configured.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var username = configuration["AdminUser:Username"];
        var password = configuration["AdminUser:Password"];
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            logger.LogWarning(
                "AdminUser:Username/Password not configured - skipping admin user seed. " +
                "Set both via 'dotnet user-secrets set' to enable login.");
            return;
        }

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var existing = await userManager.FindByNameAsync(username);
        if (existing is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = username,
            DisplayName = "Admin",
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to seed admin user: {Errors}", errors);
            return;
        }

        await userManager.AddToRoleAsync(user, "Admin");
        logger.LogInformation("Seeded admin user {Username}", username);
    }
}
