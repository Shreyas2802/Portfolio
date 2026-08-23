namespace Portfolio.Domain.Entities;

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;

    /// <summary>
    /// One contribution bullet per line - deliberately not a separate
    /// child table. A handful of short lines per project doesn't
    /// justify the extra join/CRUD complexity a normalized
    /// ProjectContribution table would add; splitting on newline is a
    /// one-line operation on both ends.
    /// </summary>
    public string ContributionsText { get; set; } = string.Empty;

    /// <summary>Comma-separated, e.g. ".NET 9, ASP.NET Core MVC, EF Core 9, SQL Server".</summary>
    public string Technologies { get; set; } = string.Empty;

    public string? GithubUrl { get; set; }
    public string? LiveUrl { get; set; }
    public string? CoverImageUrl { get; set; }

    /// <summary>The two case studies shown prominently vs. the smaller "other work" cards.</summary>
    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
