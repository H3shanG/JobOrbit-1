using JobOrbit.Application.DTOs.RecruiterAnalytics;

namespace JobOrbit.Application.Interfaces;

public interface IRecruiterAnalyticsService
{
    Task<RecruiterAnalyticsDto> GetAsync(int userId, DateTime from, DateTime to, CancellationToken token = default);
}
