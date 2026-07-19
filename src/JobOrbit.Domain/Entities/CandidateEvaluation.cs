using JobOrbit.Domain.Common;
using JobOrbit.Domain.Enums;

namespace JobOrbit.Domain.Entities;

public sealed class CandidateEvaluation : BaseEntity
{
    public int JobApplicationId { get; set; }

    public int? RecruiterProfileId { get; set; }
    public int? EvaluatorUserId { get; set; }

    public int? InterviewId { get; set; }

    public decimal OverallScore { get; set; }
    public int? TechnicalScore { get; set; }
    public int? CommunicationScore { get; set; }
    public int? ExperienceScore { get; set; }
    public int? CultureFitScore { get; set; }
    public EvaluationRecommendation? Recommendation { get; set; }

    public string? Comments { get; set; }

    public HiringDecision HiringDecision { get; set; } = HiringDecision.Pending;

    public JobApplication JobApplication { get; set; } = null!;

    public RecruiterProfile? RecruiterProfile { get; set; }
    public User? EvaluatorUser { get; set; }

    public Interview? Interview { get; set; }
}
