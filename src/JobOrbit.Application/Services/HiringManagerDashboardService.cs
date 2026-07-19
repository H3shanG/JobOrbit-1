using JobOrbit.Application.DTOs.Dashboard;
using JobOrbit.Application.Interfaces;

namespace JobOrbit.Application.Services;

public sealed class HiringManagerDashboardService(IHiringManagerDashboardRepository repository)
    : IHiringManagerDashboardService
{
    public Task<HiringManagerDashboardStatsDto?> GetStatsAsync(int userId, CancellationToken token = default) =>
        repository.GetStatsAsync(userId, token);
}
