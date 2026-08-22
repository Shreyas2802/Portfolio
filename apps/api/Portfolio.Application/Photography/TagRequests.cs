using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.Photography;

public class TagRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
}

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int PhotoCount { get; set; }
}
