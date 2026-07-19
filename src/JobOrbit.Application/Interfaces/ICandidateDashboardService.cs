using JobOrbit.Application.DTOs.Dashboard;

namespace JobOrbit.Application.Interfaces;

public interface ICandidateDashboardService
{
    Task<CandidateDashboardStatsDto> GetStatsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecentApplicationDto>> GetRecentApplicationsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendedJobDto>> GetRecommendedJobsAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
