using JobOrbit.Application.DTOs.RecruiterAnalytics;
using JobOrbit.Application.Interfaces;

namespace JobOrbit.Application.Services;

public sealed class RecruiterAnalyticsService(IRecruiterAnalyticsRepository repository) : IRecruiterAnalyticsService
{
    public Task<RecruiterAnalyticsDto> GetAsync(int userId, DateTime from, DateTime to, CancellationToken token = default) =>
        repository.GetAsync(userId, from.ToUniversalTime(), to.ToUniversalTime(), token);
}
