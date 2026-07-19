using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.Application.DTOs.HiringManagerInterviews;

public sealed class HiringManagerInterviewQuery
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public int? JobId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = "upcoming";
}
public sealed class HiringManagerInterviewListItemDto
{
    public int InterviewId { get; init; } public int ApplicationId { get; init; } public int CandidateId { get; init; }
    public string CandidateName { get; init; } = ""; public string? ProfessionalTitle { get; init; }
    public int JobId { get; init; } public string JobTitle { get; init; } = ""; public string OrganizationName { get; init; } = "";
    public DateTime ScheduledAt { get; init; } public int DurationMinutes { get; init; } public string? Location { get; init; }
    public string? MeetingLink { get; init; } public string Status { get; init; } = ""; public string ApplicationStatus { get; init; } = "";
}
public sealed class HiringManagerInterviewCandidateDto
{ public int CandidateId { get; init; } public string FullName { get; init; } = ""; public string Email { get; init; } = ""; public string? Phone { get; init; } public string? ProfessionalTitle { get; init; } public IReadOnlyList<string> Skills { get; init; } = []; }
public sealed class HiringManagerInterviewJobDto
{ public int JobId { get; init; } public string Title { get; init; } = ""; public string OrganizationName { get; init; } = ""; public string DepartmentName { get; init; } = ""; }
public sealed class HiringManagerInterviewDetailsDto
{
    public int InterviewId { get; init; } public int ApplicationId { get; init; }
    public HiringManagerInterviewCandidateDto Candidate { get; init; } = new(); public HiringManagerInterviewJobDto Job { get; init; } = new();
    public DateTime ScheduledAt { get; init; } public int DurationMinutes { get; init; } public string? Location { get; init; }
    public string? MeetingLink { get; init; } public string Status { get; init; } = ""; public string ApplicationStatus { get; init; } = "";
    public string? Notes { get; init; } public DateTime CreatedAt { get; init; } public DateTime UpdatedAt { get; init; }
}
