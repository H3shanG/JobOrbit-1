using JobOrbit.Application.DTOs.Dashboard;
using JobOrbit.Application.Interfaces;

namespace JobOrbit.Application.Services;

public sealed class RecruiterDashboardService(IRecruiterDashboardRepository repository) : IRecruiterDashboardService
{
    public Task<RecruiterDashboardStatsDto?> GetStatsAsync(int userId, CancellationToken token = default) => repository.GetStatsAsync(userId, token);
    public Task<IReadOnlyList<RecruiterRecentApplicantDto>> GetRecentApplicantsAsync(int userId, CancellationToken token = default) => repository.GetRecentApplicantsAsync(userId, token);
    public Task<IReadOnlyList<RecruiterUpcomingInterviewDto>> GetUpcomingInterviewsAsync(int userId, CancellationToken token = default) => repository.GetUpcomingInterviewsAsync(userId, token);
    public Task<RecruiterApplicationsOverviewDto> GetApplicationsOverviewAsync(int userId, CancellationToken token = default) => repository.GetApplicationsOverviewAsync(userId, token);
}
