using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.Application.DTOs.HiringManagerCandidates;

public sealed class HiringManagerCandidateQuery
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public int? JobId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = "newest";
}

public sealed class HiringManagerCandidateListItemDto
{
    public int ApplicationId { get; init; }
    public int CandidateId { get; init; }
    public string CandidateName { get; init; } = "";
    public string? ProfessionalTitle { get; init; }
    public int JobId { get; init; }
    public string JobTitle { get; init; } = "";
    public string Status { get; init; } = "";
    public DateTime AppliedOn { get; init; }
    public decimal? MatchScore { get; init; }
    public string? InterviewStatus { get; init; }
    public string EvaluationStatus { get; init; } = "Pending";
}

public sealed class HiringManagerCandidateSummaryDto
{
    public int CandidateId { get; init; }
    public string FullName { get; init; } = "";
    public string Email { get; init; } = "";
    public string? Phone { get; init; }
    public string? ProfessionalTitle { get; init; }
    public string? ProfessionalSummary { get; init; }
    public string? Education { get; init; }
    public string? Experience { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? PortfolioUrl { get; init; }
    public IReadOnlyList<string> Skills { get; init; } = [];
}

public sealed class HiringManagerJobSummaryDto
{
    public int JobId { get; init; }
    public string Title { get; init; } = "";
    public string? DepartmentName { get; init; }
    public string Location { get; init; } = "";
    public string EmploymentType { get; init; } = "";
    public string? Requirements { get; init; }
}

public sealed class HiringManagerResumeSummaryDto
{
    public int ResumeId { get; init; }
    public string DisplayName { get; init; } = "";
    public string OriginalFileName { get; init; } = "";
}

public sealed class HiringManagerInterviewSummaryDto
{
    public int InterviewId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public string? Location { get; init; }
    public string? MeetingLink { get; init; }
    public string Status { get; init; } = "";
}

public sealed class HiringManagerCandidateEvaluationSummaryDto
{
    public int EvaluationId { get; init; }
    public decimal OverallScore { get; init; }
    public string? Comments { get; init; }
    public string HiringDecision { get; init; } = "Pending";
    public string EvaluatorName { get; init; } = "";
    public DateTime EvaluatedAt { get; init; }
    public bool CanEdit { get; init; }
}

public sealed class HiringManagerCandidateDetailsDto
{
    public int ApplicationId { get; init; }
    public string Status { get; init; } = "";
    public DateTime AppliedOn { get; init; }
    public string? CoverLetter { get; init; }
    public HiringManagerCandidateSummaryDto Candidate { get; init; } = new();
    public HiringManagerJobSummaryDto Job { get; init; } = new();
    public HiringManagerResumeSummaryDto? Resume { get; init; }
    public HiringManagerInterviewSummaryDto? Interview { get; init; }
    public HiringManagerCandidateEvaluationSummaryDto? ExistingEvaluation { get; init; }
}

public sealed class HiringManagerDashboardCandidateDto
{
    public int ApplicationId { get; init; }
    public string CandidateName { get; init; } = "";
    public string? ProfessionalTitle { get; init; }
    public string JobTitle { get; init; } = "";
    public decimal? MatchScore { get; init; }
    public DateTime AppliedOn { get; init; }
}

public sealed record HiringManagerResumeDownloadDto(Stream Content, string ContentType, string OriginalFileName);
public sealed record HiringManagerResumeFileDto(string StoredFileName, string ContentType, string OriginalFileName);
