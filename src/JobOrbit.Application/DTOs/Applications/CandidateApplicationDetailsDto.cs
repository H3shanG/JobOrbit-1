namespace JobOrbit.Application.DTOs.Applications;

public sealed class CandidateApplicationDetailsDto
{
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public DateTime AppliedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public CandidateInterviewSummaryDto? Interview { get; set; }
    public IReadOnlyList<ApplicationTimelineItemDto> Timeline { get; set; } = [];
}

public sealed class CandidateInterviewSummaryDto
{
    public int InterviewId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class ApplicationTimelineItemDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
}
