using JobOrbit.Application.DTOs.HiringManagerCandidates;
using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.Application.Interfaces;

public interface IHiringManagerCandidateRepository
{
    Task<PagedResultDto<HiringManagerCandidateListItemDto>> ListAsync(int userId, HiringManagerCandidateQuery query, CancellationToken token = default);
    Task<HiringManagerCandidateDetailsDto?> DetailsAsync(int userId, int applicationId, CancellationToken token = default);
    Task<HiringManagerResumeFileDto?> ResumeAsync(int userId, int applicationId, CancellationToken token = default);
    Task<IReadOnlyList<HiringManagerDashboardCandidateDto>> LatestAsync(int userId, CancellationToken token = default);
}
