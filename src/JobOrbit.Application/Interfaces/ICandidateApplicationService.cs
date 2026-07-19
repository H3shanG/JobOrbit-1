using JobOrbit.Application.DTOs.Applications;

namespace JobOrbit.Application.Interfaces;

public interface ICandidateApplicationService
{
    Task<CandidateApplicationsPageDto> GetApplicationsAsync(
        int userId,
        CandidateApplicationQueryDto query,
        CancellationToken cancellationToken = default);

    Task<CandidateApplicationDetailsDto?> GetApplicationAsync(
        int userId,
        int applicationId,
        CancellationToken cancellationToken = default);
}
