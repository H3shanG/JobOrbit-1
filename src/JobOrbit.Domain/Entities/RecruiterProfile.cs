using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class RecruiterProfile : BaseEntity
{
    public int UserId { get; set; }

    public int OrganizationId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public bool JobApplicationNotifications { get; set; } = true;

    public bool InterviewNotifications { get; set; } = true;

    public bool CandidateStatusNotifications { get; set; } = true;

    public bool EmailNotifications { get; set; } = true;

    public User User { get; set; } = null!;

    public Organization Organization { get; set; } = null!;

    public ICollection<JobPosting> JobPostings { get; set; } = [];

    public ICollection<CandidateEvaluation> CandidateEvaluations { get; set; } = [];
}
