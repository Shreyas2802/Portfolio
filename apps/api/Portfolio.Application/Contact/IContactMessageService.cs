using Portfolio.Application.Common;

namespace Portfolio.Application.Contact;

public interface IContactMessageService
{
    /// <returns>false if silently dropped (honeypot triggered).</returns>
    Task<bool> SubmitAsync(SubmitContactMessageRequest request, CancellationToken cancellationToken);

    Task<PagedResult<ContactMessageDto>> GetMessagesAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<bool> MarkReadAsync(int id, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
