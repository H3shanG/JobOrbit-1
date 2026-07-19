using JobOrbit.Application.DTOs.HiringManagerEvaluations;
using JobOrbit.Domain.Enums;

namespace JobOrbit.Application.Interfaces;

public interface IHiringManagerEvaluationRepository
{
    Task<EvaluationMutationResult> CreateAsync(int userId, int applicationId, CandidateEvaluationRequest request, EvaluationRecommendation recommendation, decimal overallScore, CancellationToken token = default);
    Task<IReadOnlyList<CandidateEvaluationDto>?> ListAsync(int userId, int applicationId, CancellationToken token = default);
    Task<EvaluationMutationResult> UpdateAsync(int userId, int evaluationId, CandidateEvaluationRequest request, EvaluationRecommendation recommendation, decimal overallScore, CancellationToken token = default);
    Task<HiringManagerEvaluationSummaryDto> SummaryAsync(int userId, CancellationToken token = default);
}
