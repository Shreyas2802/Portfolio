namespace Portfolio.Domain.Entities;

/// <summary>
/// A message submitted through the public Contact form. Per the
/// spec's domain model (section 6). Admin CRUD/inbox for these
/// arrives with the rest of the admin app in Phase 6 - for now this
/// just needs to be persisted safely.
/// </summary>
public class ContactMessage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
    public bool IsRead { get; set; }
}
