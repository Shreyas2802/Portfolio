using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.Photography;

public class AlbumCreateRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public int? CategoryId { get; set; }
    public bool IsPublished { get; set; } = true;
}

public class AlbumUpdateRequest : AlbumCreateRequest
{
}

public class AlbumAdminDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsPublished { get; set; }
    public int PhotoCount { get; set; }
}
