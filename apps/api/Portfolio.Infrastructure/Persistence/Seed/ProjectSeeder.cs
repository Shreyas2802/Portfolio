using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds the project case studies that previously lived as static
/// frontend data (src/data/projects.ts, Phase 3) - now that Project
/// management (Phase 6) exists, this is a one-time migration of that
/// content into the database so the public Projects page doesn't go
/// blank when it switches from static data to an API call. Runs once;
/// no-op if any project already exists (admin-created content is
/// never overwritten).
/// </summary>
public static class ProjectSeeder
{
    public static async Task SeedAsync(PortfolioDbContext dbContext)
    {
        if (dbContext.Projects.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var projects = new List<Project>
        {
            new()
            {
                Title = "Brahmanda Tatva",
                Slug = "brahmanda-tatva",
                Tagline = "Multi-tenant authentication & authorization platform",
                Role = "Requirements, UI implementation, and QA",
                Status = "MVP reached — additional modules planned before commercial launch",
                Overview = "An internal platform handling authentication, role-based access control, " +
                    "tenant provisioning, and invitation workflows across multiple tenants and products. " +
                    "Built from early development through MVP.",
                ContributionsText = string.Join('\n',
                    "Defined functional requirements and behavior specifications for authentication, role-based access control, tenant provisioning, and invitation workflows, working directly with the development team to translate them into technical specs.",
                    "Implemented UI screens across multiple product modules by adapting and replicating the platform's design system, keeping visual and functional behavior consistent across tenants.",
                    "Owned functional and validation testing for authentication, authorization, and tenant-management flows using Postman, identifying issues ahead of release.",
                    "Drove the project day-to-day with architectural guidance and test-scenario input from a senior developer."),
                Technologies = ".NET 9, ASP.NET Core MVC, EF Core 9, SQL Server, Postman",
                IsFeatured = true,
                IsPublished = true,
                DisplayOrder = 0,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            },
            new()
            {
                Title = "Portfolio Platform",
                Slug = "portfolio-platform",
                Tagline = "This site — photography portfolio, dev portfolio, and admin CMS",
                Role = "Architecture, full-stack implementation, and design",
                Status = "In active development, built phase by phase",
                Overview = "A full-stack site combining a photography portfolio, a software-development " +
                    "portfolio, and a private admin CMS — built as a clean-architecture monorepo (Domain / " +
                    "Application / Infrastructure / Api on the backend, a typed React SPA on the frontend) " +
                    "to be a real, working demonstration of both skill sets rather than a template.",
                ContributionsText = string.Join('\n',
                    "Designed the monorepo and clean-architecture project structure, keeping business logic out of both the database layer and the UI.",
                    "Built the design system from scratch — palette, typography, and a signature EXIF-style technical-label device tying the photography and engineering sides of the site together.",
                    "Wired up EF Core, SQL Server migrations, Identity/JWT authentication, and a full admin CMS for photos, albums, categories, tags, and projects.",
                    "Following a phased build plan (foundation → design system → backend → content → photography → admin → hardening → deployment), verifying each phase locally before moving to the next."),
                Technologies = "React, TypeScript, Vite, Tailwind CSS, ASP.NET Core, EF Core, SQL Server",
                IsFeatured = true,
                IsPublished = true,
                DisplayOrder = 1,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            },
            new()
            {
                Title = "AuthServer Authentication Platform",
                Slug = "authserver",
                Tagline = "Internal authentication server for user login and access management",
                Role = "API testing, JWT workflow validation, UI improvements",
                Status = "Internal tool, in use",
                Overview = "Internal authentication server handling user login and access management " +
                    "across internal tools.",
                ContributionsText = string.Join('\n',
                    "Performed API testing and validation using Swagger and Postman.",
                    "Tested JWT-based authentication workflows end to end.",
                    "Contributed UI improvements and bug fixes; documented authentication-related features."),
                Technologies = ".NET 8, ASP.NET Core Web API, SQL Server, JWT, Swagger",
                IsFeatured = false,
                IsPublished = true,
                DisplayOrder = 2,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            },
            new()
            {
                Title = "Jyoti Ashram Website",
                Slug = "jyoti-ashram-website",
                Tagline = "Responsive client website for events, donations, and admin functionality",
                Role = "UI/UX, responsiveness, payment integration support",
                Status = "Live, in production",
                Overview = "Responsive website for a client to manage events, donations, public " +
                    "information, and admin functionality.",
                ContributionsText = string.Join('\n',
                    "Improved UI and responsiveness using Tailwind CSS.",
                    "Helped integrate event management via FullCalendar.",
                    "Supported payment gateway integration and testing; worked with senior developers on deployment and bug fixes."),
                Technologies = ".NET 7, ASP.NET Core MVC, SQL Server, Tailwind CSS, Bootstrap, Razorpay, Azure",
                IsFeatured = false,
                IsPublished = true,
                DisplayOrder = 3,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            },
            new()
            {
                Title = "Vistrut Task Tracker",
                Slug = "vistrut-task-tracker",
                Tagline = "Internal task management application for workflow and task tracking",
                Role = "Front-end enhancements, backend support, testing",
                Status = "Internal tool, in use",
                Overview = "Internal task management application for workflow and task tracking across teams.",
                ContributionsText = string.Join('\n',
                    "Implemented front-end enhancements and responsiveness improvements.",
                    "Supported backend feature work and bug fixing.",
                    "Tested application functionality and verified workflows across modules alongside senior developers."),
                Technologies = ".NET 7, ASP.NET Core MVC, SQL Server, Bootstrap, Knockout.js",
                IsFeatured = false,
                IsPublished = true,
                DisplayOrder = 4,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            },
        };

        dbContext.Projects.AddRange(projects);
        await dbContext.SaveChangesAsync();
    }
}
