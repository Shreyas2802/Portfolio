using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.Photography;

public class CategoryRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int AlbumCount { get; set; }
}
