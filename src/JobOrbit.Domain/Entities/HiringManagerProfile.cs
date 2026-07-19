using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class HiringManagerProfile : BaseEntity
{
    public int UserId { get; set; }
    public int OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
    public string JobTitle { get; set; } = "Hiring Manager";
    public string? PhoneNumber { get; set; }
    public bool CandidateReviewNotifications { get; set; } = true;
    public bool InterviewNotifications { get; set; } = true;
    public bool EvaluationNotifications { get; set; } = true;
    public bool DecisionNotifications { get; set; } = true;
    public bool EmailNotifications { get; set; } = true;
    public User User { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public Department? Department { get; set; }
}
