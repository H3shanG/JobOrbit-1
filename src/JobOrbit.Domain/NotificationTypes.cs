namespace JobOrbit.Domain;

public static class NotificationTypes
{
    public const string ApplicationSubmitted = nameof(ApplicationSubmitted);
    public const string ApplicationStatusChanged = nameof(ApplicationStatusChanged);
    public const string InterviewScheduled = nameof(InterviewScheduled);
    public const string InterviewRescheduled = nameof(InterviewRescheduled);
    public const string InterviewCancelled = nameof(InterviewCancelled);
    public const string HiringDecisionUpdated = nameof(HiringDecisionUpdated);
    public const string NewJobRecommendation = nameof(NewJobRecommendation);
    public const string NewApplicationReceived = nameof(NewApplicationReceived);
    public const string CandidateProfileUpdated = nameof(CandidateProfileUpdated);
    public const string InterviewReminder = nameof(InterviewReminder);
    public const string InterviewStatusChanged = nameof(InterviewStatusChanged);
    public const string CandidateReadyForReview = nameof(CandidateReadyForReview);
    public const string EvaluationRequired = nameof(EvaluationRequired);
    public const string HiringDecisionRequired = nameof(HiringDecisionRequired);
    public const string CriticalAuditEvent = nameof(CriticalAuditEvent);
    public const string UserAccountIssue = nameof(UserAccountIssue);
    public const string OrganizationDeactivated = nameof(OrganizationDeactivated);
    public const string SystemMaintenanceChanged = nameof(SystemMaintenanceChanged);

    public static readonly HashSet<string> All = new(StringComparer.Ordinal)
    {
        ApplicationSubmitted, ApplicationStatusChanged, InterviewScheduled, InterviewRescheduled,
        InterviewCancelled, HiringDecisionUpdated, NewJobRecommendation, NewApplicationReceived,
        CandidateProfileUpdated, InterviewReminder, InterviewStatusChanged, CandidateReadyForReview,
        EvaluationRequired, HiringDecisionRequired, CriticalAuditEvent, UserAccountIssue,
        OrganizationDeactivated, SystemMaintenanceChanged
    };
}
