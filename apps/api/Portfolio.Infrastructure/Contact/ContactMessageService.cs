using Portfolio.Application.Contact;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Contact;

public class ContactMessageService : IContactMessageService
{
    private readonly PortfolioDbContext _dbContext;

    public ContactMessageService(PortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> SubmitAsync(SubmitContactMessageRequest request, CancellationToken cancellationToken)
    {
        // Honeypot: a real visitor never fills this field (it's hidden
        // via CSS on the frontend). If it's non-empty, silently pretend
        // success without touching the database.
        if (!string.IsNullOrEmpty(request.Website))
        {
            return false;
        }

        var message = new ContactMessage
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Message = request.Message.Trim(),
            SubmittedAtUtc = DateTime.UtcNow,
            IsRead = false,
        };

        _dbContext.ContactMessages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
