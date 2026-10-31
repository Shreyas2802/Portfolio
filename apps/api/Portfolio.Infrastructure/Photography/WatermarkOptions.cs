namespace Portfolio.Infrastructure.Photography;

/// <summary>
/// Bound from the "Watermark" section in configuration.
/// Set via web.config env vars in production:
///   Watermark__Enabled = true
///   Watermark__Text    = © Shreyas
/// Defaults to disabled so existing deployments are unaffected
/// until explicitly turned on.
/// </summary>
public class WatermarkOptions
{
    public bool   Enabled { get; set; } = false;
    public string Text    { get; set; } = "© Shreyas";
}
