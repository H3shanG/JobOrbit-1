using JobOrbit.Application.DTOs.HiringManagerEvaluations;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;

namespace JobOrbit.Application.Services;

public sealed class HiringManagerEvaluationService(IHiringManagerEvaluationRepository repository) : IHiringManagerEvaluationService
{
    private static bool TryPrepare(CandidateEvaluationRequest request, out EvaluationRecommendation recommendation, out decimal overall)
    {
        overall = Math.Round((request.TechnicalScore + request.CommunicationScore + request.ExperienceScore + request.CultureFitScore) / 4m, 2, MidpointRounding.AwayFromZero);
        return Enum.TryParse(request.Recommendation, true, out recommendation);
    }
    public Task<EvaluationMutationResult> CreateAsync(int userId, int applicationId, CreateCandidateEvaluationRequest request, CancellationToken token = default) =>
        TryPrepare(request, out var recommendation, out var overall) ? repository.CreateAsync(userId, applicationId, request, recommendation, overall, token) : Task.FromResult(new EvaluationMutationResult(EvaluationMutationOutcome.InvalidRecommendation));
    public Task<IReadOnlyList<CandidateEvaluationDto>?> ListAsync(int userId, int applicationId, CancellationToken token = default) => repository.ListAsync(userId, applicationId, token);
    public Task<EvaluationMutationResult> UpdateAsync(int userId, int evaluationId, UpdateCandidateEvaluationRequest request, CancellationToken token = default) =>
        TryPrepare(request, out var recommendation, out var overall) ? repository.UpdateAsync(userId, evaluationId, request, recommendation, overall, token) : Task.FromResult(new EvaluationMutationResult(EvaluationMutationOutcome.InvalidRecommendation));
    public Task<HiringManagerEvaluationSummaryDto> SummaryAsync(int userId, CancellationToken token = default) => repository.SummaryAsync(userId, token);
}
