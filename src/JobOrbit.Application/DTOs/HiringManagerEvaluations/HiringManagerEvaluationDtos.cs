using System.ComponentModel.DataAnnotations;

namespace JobOrbit.Application.DTOs.HiringManagerEvaluations;

public class CandidateEvaluationRequest
{
    [Range(1, 10)] public int TechnicalScore { get; init; }
    [Range(1, 10)] public int CommunicationScore { get; init; }
    [Range(1, 10)] public int ExperienceScore { get; init; }
    [Range(1, 10)] public int CultureFitScore { get; init; }
    [MaxLength(4000)] public string? OverallComments { get; init; }
    [Required] public string Recommendation { get; init; } = "";
}
public sealed class CreateCandidateEvaluationRequest : CandidateEvaluationRequest;
public sealed class UpdateCandidateEvaluationRequest : CandidateEvaluationRequest;

public sealed class CandidateEvaluationDto
{
    public int EvaluationId { get; init; }
    public int ApplicationId { get; init; }
    public int TechnicalScore { get; init; }
    public int CommunicationScore { get; init; }
    public int ExperienceScore { get; init; }
    public int CultureFitScore { get; init; }
    public decimal OverallScore { get; init; }
    public string? OverallComments { get; init; }
    public string Recommendation { get; init; } = "";
    public string EvaluatorName { get; init; } = "";
    public bool CanEdit { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class EvaluationRecommendationCountDto { public string Recommendation { get; init; } = ""; public int Count { get; init; } }
public sealed class HiringManagerEvaluationSummaryDto
{
    public decimal AverageOverallScore { get; init; }
    public int CompletedEvaluations { get; init; }
    public int PendingEvaluations { get; init; }
    public IReadOnlyList<EvaluationRecommendationCountDto> RecommendationCounts { get; init; } = [];
}
public enum EvaluationMutationOutcome { Success, NotFound, Duplicate, InvalidState, InvalidRecommendation }
public sealed record EvaluationMutationResult(EvaluationMutationOutcome Outcome, CandidateEvaluationDto? Evaluation = null);
