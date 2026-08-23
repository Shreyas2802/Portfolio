using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.Projects;

public class ProjectRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string Tagline { get; set; } = string.Empty;

    [StringLength(300)]
    public string Role { get; set; } = string.Empty;

    [StringLength(300)]
    public string Status { get; set; } = string.Empty;

    [StringLength(4000)]
    public string Overview { get; set; } = string.Empty;

    /// <summary>One contribution bullet per line.</summary>
    [StringLength(4000)]
    public string ContributionsText { get; set; } = string.Empty;

    /// <summary>Comma-separated technology names.</summary>
    [StringLength(500)]
    public string Technologies { get; set; } = string.Empty;

    [StringLength(500)]
    public string? GithubUrl { get; set; }

    [StringLength(500)]
    public string? LiveUrl { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; } = true;
}
