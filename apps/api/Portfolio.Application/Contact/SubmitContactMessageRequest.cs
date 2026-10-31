using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.Contact;

/// <summary>
/// Input for submitting the contact form. DataAnnotations here are
/// enforced server-side via ModelState (the API never trusts that the
/// frontend's Zod validation actually ran - spec principle #2).
/// </summary>
public class SubmitContactMessageRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(4000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Honeypot field. Real users never see or fill this (it's hidden
    /// via CSS on the frontend); if it arrives non-empty, the
    /// submission is silently dropped. A cheap first line of defense
    /// against basic bots - real rate limiting arrives in Phase 7.
    /// </summary>
    public string? Website { get; set; }
}
