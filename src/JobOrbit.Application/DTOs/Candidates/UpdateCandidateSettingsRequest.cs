namespace JobOrbit.Application.DTOs.Candidates;

public sealed class UpdateCandidateSettingsRequest
{
    public bool EmailNotifications { get; init; }
    public bool ApplicationStatusNotifications { get; init; }
    public bool InterviewReminders { get; init; }
    public bool JobRecommendationNotifications { get; init; }
}
