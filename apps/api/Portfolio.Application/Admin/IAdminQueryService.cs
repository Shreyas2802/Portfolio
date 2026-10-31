namespace Portfolio.Application.Admin;

public interface IAdminQueryService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken);
}
