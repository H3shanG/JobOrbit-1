using JobOrbit.Domain.Common;
using JobOrbit.Domain.Enums;

namespace JobOrbit.Domain.Entities;

public sealed class JobApplication : BaseEntity
{
    public int JobPostingId { get; set; }

    public int CandidateProfileId { get; set; }

    public string? CoverLetter { get; set; }

    public string? ResumeUrl { get; set; }

    public int? ResumeId { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;

    public DateTime AppliedAt { get; set; }

    public JobPosting JobPosting { get; set; } = null!;

    public CandidateProfile CandidateProfile { get; set; } = null!;

    public Resume? Resume { get; set; }

    public ICollection<Interview> Interviews { get; set; } = [];

    public ICollection<CandidateEvaluation> CandidateEvaluations { get; set; } = [];
    public ApplicationHiringDecision? HiringDecision { get; set; }
}
