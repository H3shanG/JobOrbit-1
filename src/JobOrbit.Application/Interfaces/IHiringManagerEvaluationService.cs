using JobOrbit.Application.DTOs.HiringManagerEvaluations;

namespace JobOrbit.Application.Interfaces;

public interface IHiringManagerEvaluationService
{
    Task<EvaluationMutationResult> CreateAsync(int userId, int applicationId, CreateCandidateEvaluationRequest request, CancellationToken token = default);
    Task<IReadOnlyList<CandidateEvaluationDto>?> ListAsync(int userId, int applicationId, CancellationToken token = default);
    Task<EvaluationMutationResult> UpdateAsync(int userId, int evaluationId, UpdateCandidateEvaluationRequest request, CancellationToken token = default);
    Task<HiringManagerEvaluationSummaryDto> SummaryAsync(int userId, CancellationToken token = default);
}
