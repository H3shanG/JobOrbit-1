using JobOrbit.Application.DTOs.Dashboard;
using JobOrbit.Application.Interfaces;

namespace JobOrbit.Application.Services;

public sealed class CandidateDashboardService(
    ICandidateDashboardRepository dashboardRepository)
    : ICandidateDashboardService
{
    public Task<CandidateDashboardStatsDto> GetStatsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return dashboardRepository.GetStatsAsync(userId, cancellationToken);
    }

    public Task<IReadOnlyList<RecentApplicationDto>> GetRecentApplicationsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return dashboardRepository.GetRecentApplicationsAsync(userId, cancellationToken);
    }

    public Task<IReadOnlyList<RecommendedJobDto>> GetRecommendedJobsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return dashboardRepository.GetRecommendedJobsAsync(userId, cancellationToken);
    }
}
