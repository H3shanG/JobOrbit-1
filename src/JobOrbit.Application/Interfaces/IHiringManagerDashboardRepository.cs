using JobOrbit.Application.DTOs.Dashboard;

namespace JobOrbit.Application.Interfaces;

public interface IHiringManagerDashboardRepository
{
    Task<HiringManagerDashboardStatsDto?> GetStatsAsync(int userId, CancellationToken token = default);
}
