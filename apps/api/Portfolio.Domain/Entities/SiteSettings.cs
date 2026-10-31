namespace Portfolio.Domain.Entities;

/// <summary>
/// Site-wide settings, per the spec's domain model (section 6). Kept
/// deliberately small right now - this exists in Phase 2 purely as
/// the first real entity to prove Domain -> Infrastructure -> EF Core
/// -> SQL Server works end to end, before Photo/Album/Project (which
/// need image processing and upload handling) arrive in later phases.
/// </summary>
public class SiteSettings
{
    public int Id { get; set; }
    public string SiteTitle { get; set; } = string.Empty;
    public string SiteDescription { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
}
