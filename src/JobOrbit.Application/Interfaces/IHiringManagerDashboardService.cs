using JobOrbit.Application.DTOs.Dashboard;

namespace JobOrbit.Application.Interfaces;

public interface IHiringManagerDashboardService
{
    Task<HiringManagerDashboardStatsDto?> GetStatsAsync(int userId, CancellationToken token = default);
}
