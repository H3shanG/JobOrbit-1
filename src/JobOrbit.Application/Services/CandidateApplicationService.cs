using JobOrbit.Application.DTOs.Applications;
using JobOrbit.Application.Interfaces;

namespace JobOrbit.Application.Services;

public sealed class CandidateApplicationService(
    ICandidateApplicationRepository repository) : ICandidateApplicationService
{
    public Task<CandidateApplicationsPageDto> GetApplicationsAsync(
        int userId,
        CandidateApplicationQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 50);
        query.Sort = string.IsNullOrWhiteSpace(query.Sort) ? "newest" : query.Sort.Trim();
        return repository.GetApplicationsAsync(userId, query, cancellationToken);
    }

    public Task<CandidateApplicationDetailsDto?> GetApplicationAsync(
        int userId,
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        return repository.GetApplicationAsync(userId, applicationId, cancellationToken);
    }
}
