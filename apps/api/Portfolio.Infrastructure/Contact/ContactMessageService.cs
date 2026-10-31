using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common;
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
        if (!string.IsNullOrEmpty(request.Website))
            return false;

        _dbContext.ContactMessages.Add(new ContactMessage
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Message = request.Message.Trim(),
            SubmittedAtUtc = DateTime.UtcNow,
            IsRead = false,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PagedResult<ContactMessageDto>> GetMessagesAsync(
        int page, int pageSize, CancellationToken cancellationToken)
    {
        page = PaginationLimits.ClampPage(page);
        pageSize = PaginationLimits.ClampPageSize(pageSize);

        var query = _dbContext.ContactMessages
            .OrderByDescending(m => m.SubmittedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ContactMessageDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public async Task<bool> MarkReadAsync(int id, CancellationToken cancellationToken)
    {
        var msg = await _dbContext.ContactMessages.FindAsync(new object[] { id }, cancellationToken);
        if (msg is null) return false;
        msg.IsRead = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var msg = await _dbContext.ContactMessages.FindAsync(new object[] { id }, cancellationToken);
        if (msg is null) return false;
        _dbContext.ContactMessages.Remove(msg);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ContactMessageDto ToDto(ContactMessage m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Email = m.Email,
        Message = m.Message,
        SubmittedAtUtc = m.SubmittedAtUtc,
        IsRead = m.IsRead,
    };
}
