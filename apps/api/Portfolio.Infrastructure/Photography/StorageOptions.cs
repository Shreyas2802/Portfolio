namespace Portfolio.Infrastructure.Photography;

/// <summary>
/// Bound from the "Storage" config section. UploadsRootPath is
/// optional - when unset, storage falls back to wwwroot/uploads
/// (unchanged dev behavior, so nothing already on disk needs moving).
/// In production, this should point OUTSIDE the deployed app's
/// publish folder - see the Phase 10 deployment guide for why:
/// republishing typically replaces the app's folder contents wholesale,
/// which would silently delete every uploaded photo if they lived
/// inside it.
/// </summary>
public class StorageOptions
{
    public string? UploadsRootPath { get; set; }
}
