using JobOrbit.Application.DTOs.Dashboard;

namespace JobOrbit.Application.Interfaces;

public interface IRecruiterDashboardRepository
{
    Task<RecruiterDashboardStatsDto?> GetStatsAsync(int userId, CancellationToken token = default);
    Task<IReadOnlyList<RecruiterRecentApplicantDto>> GetRecentApplicantsAsync(int userId, CancellationToken token = default);
    Task<IReadOnlyList<RecruiterUpcomingInterviewDto>> GetUpcomingInterviewsAsync(int userId, CancellationToken token = default);
    Task<RecruiterApplicationsOverviewDto> GetApplicationsOverviewAsync(int userId, CancellationToken token = default);
}
