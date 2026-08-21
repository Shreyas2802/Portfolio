using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Admin;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Admin;

public class AdminQueryService : IAdminQueryService
{
    private readonly PortfolioDbContext _dbContext;

    public AdminQueryService(PortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken)
    {
        return new DashboardStatsDto
        {
            PhotoCount = await _dbContext.Photos.CountAsync(cancellationToken),
            AlbumCount = await _dbContext.Albums.CountAsync(cancellationToken),
            ProjectCount = await _dbContext.Projects.CountAsync(cancellationToken),
            UnreadContactMessageCount = await _dbContext.ContactMessages.CountAsync(m => !m.IsRead, cancellationToken),
        };
    }
}
