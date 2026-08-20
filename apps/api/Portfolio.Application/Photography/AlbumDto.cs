namespace Portfolio.Application.Photography;

public class AlbumSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? CoverThumbnailUrl { get; set; }
    public int PhotoCount { get; set; }
}

public class AlbumDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public List<PhotoDto> Photos { get; set; } = new();
}
