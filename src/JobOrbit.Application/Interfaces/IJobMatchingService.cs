using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.Matching;

namespace JobOrbit.Application.Interfaces;

public interface IJobMatchingService
{
 Task<JobMatchResultDto?> CalculateCandidateJobMatchAsync(int candidateUserId,int jobId,CancellationToken token=default);
 Task<IReadOnlyList<JobRecommendationDto>> GetRecommendedJobsAsync(int candidateUserId,CandidateRecommendationFilter filter,CancellationToken token=default);
 Task<PagedResultDto<RankedCandidateDto>?> GetRankedApplicantsAsync(int recruiterUserId,int jobId,CandidateRankingFilter filter,CancellationToken token=default);
 Task<JobMatchResultDto?> GetRecruiterApplicationMatchAsync(int recruiterUserId,int applicationId,CancellationToken token=default);
 Task<JobMatchResultDto?> GetManagerApplicationMatchAsync(int managerUserId,int applicationId,CancellationToken token=default);
}
