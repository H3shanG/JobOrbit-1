namespace JobOrbit.Application.DTOs.Dashboard;

public sealed class RecruiterUpcomingInterviewDto
{
    public int InterviewId { get; init; }
    public int ApplicationId { get; init; }
    public int CandidateId { get; init; }
    public string CandidateName { get; init; } = string.Empty;
    public int JobId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public DateTime ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public string? Location { get; init; }
    public string? MeetingLink { get; init; }
    public string Status { get; init; } = string.Empty;
}
