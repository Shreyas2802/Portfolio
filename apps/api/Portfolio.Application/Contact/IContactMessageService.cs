namespace Portfolio.Application.Contact;

/// <summary>
/// Defined in Application, implemented in Infrastructure - the
/// controller depends on this interface, never on EF Core directly.
/// </summary>
public interface IContactMessageService
{
    /// <returns>false if the submission was silently dropped (honeypot triggered).</returns>
    Task<bool> SubmitAsync(SubmitContactMessageRequest request, CancellationToken cancellationToken);
}
