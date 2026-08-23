namespace Portfolio.Application.Projects;

public class ProjectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public List<string> Contributions { get; set; } = new();
    public List<string> Technologies { get; set; } = new();
    public string? GithubUrl { get; set; }
    public string? LiveUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsFeatured { get; set; }
}

/// <summary>Admin listing includes unpublished projects and raw text fields for editing.</summary>
public class ProjectAdminDto : ProjectDto
{
    public bool IsPublished { get; set; }
}
