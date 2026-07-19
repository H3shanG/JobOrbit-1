using JobOrbit.Domain.Common;
using JobOrbit.Domain.Enums;

namespace JobOrbit.Domain.Entities;

public sealed class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public bool EmailNotifications { get; set; } = true;
    public bool ApplicationStatusNotifications { get; set; } = true;
    public bool InterviewReminders { get; set; } = true;
    public bool JobRecommendationNotifications { get; set; } = true;

    public CandidateProfile? CandidateProfile { get; set; }

    public RecruiterProfile? RecruiterProfile { get; set; }
    public HiringManagerProfile? HiringManagerProfile { get; set; }
    public ICollection<CandidateEvaluation> AuthoredCandidateEvaluations { get; set; } = [];
    public ICollection<ApplicationHiringDecision> HiringDecisions { get; set; } = [];

    public ICollection<AuditLog> AuditLogs { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
