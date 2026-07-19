namespace JobOrbit.Application.DTOs.Candidates;

public sealed class CandidateSettingsDto
{
    public string Email { get; init; } = string.Empty;
    public bool EmailNotifications { get; init; }
    public bool ApplicationStatusNotifications { get; init; }
    public bool InterviewReminders { get; init; }
    public bool JobRecommendationNotifications { get; init; }
}
