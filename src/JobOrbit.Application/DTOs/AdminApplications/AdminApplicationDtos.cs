using System.ComponentModel.DataAnnotations;
using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.Application.DTOs.AdminApplications;

public sealed class AdminApplicationQuery
{
    public string? Search { get; set; } public string? Status { get; set; } public int? JobId { get; set; }
    public int? OrganizationId { get; set; } public int? DepartmentId { get; set; } public int? RecruiterId { get; set; }
    public int? CandidateId { get; set; } public string? Decision { get; set; } public DateTime? From { get; set; }
    public DateTime? To { get; set; } public int Page { get; set; }=1; public int PageSize { get; set; }=10; public string Sort { get; set; }="newest";
}
public sealed record AdminApplicationListItemDto(int ApplicationId,int CandidateId,string CandidateName,string CandidateEmail,int JobId,string JobTitle,int OrganizationId,string OrganizationName,int DepartmentId,string DepartmentName,int RecruiterId,string RecruiterName,string Status,string? InterviewStatus,string EvaluationStatus,string Decision,decimal? MatchScore,DateTime AppliedAt,DateTime UpdatedAt,int? ResumeId);
public sealed record AdminApplicationCandidateSummaryDto(int CandidateId,string FullName,string Email,string? Phone,string? ProfessionalTitle,IReadOnlyList<string> Skills);
public sealed record AdminApplicationJobSummaryDto(int JobId,string Title,string Status,string OrganizationName,string DepartmentName,string RecruiterName);
public sealed record AdminApplicationResumeSummaryDto(int ResumeId,string DisplayName,string OriginalFileName);
public sealed record AdminApplicationInterviewSummaryDto(int InterviewId,DateTime ScheduledAt,int DurationMinutes,string? Location,string Status);
public sealed record AdminApplicationEvaluationSummaryDto(int EvaluationId,string EvaluatorName,decimal OverallScore,string? Recommendation,DateTime CreatedAt);
public sealed record AdminApplicationDecisionSummaryDto(string Decision,string DecidedBy,DateTime DecidedAt);
public sealed record AdminApplicationDetailsDto(int ApplicationId,string Status,DateTime AppliedAt,DateTime UpdatedAt,string? CoverLetter,decimal? MatchScore,AdminApplicationCandidateSummaryDto Candidate,AdminApplicationJobSummaryDto Job,AdminApplicationResumeSummaryDto? Resume,IReadOnlyList<AdminApplicationInterviewSummaryDto> Interviews,IReadOnlyList<AdminApplicationEvaluationSummaryDto> Evaluations,AdminApplicationDecisionSummaryDto? HiringDecision);
public sealed record AdminApplicationHistoryItemDto(string EventType,string Description,string? ActorName,string? ActorRole,DateTime OccurredAt);
public sealed class UpdateAdminApplicationStatusRequest { [Required] public string Status { get; set; }=string.Empty; [Required,MinLength(5),MaxLength(500)] public string Reason { get; set; }=string.Empty; }
public sealed record AdminApplicationListResult(bool Valid,PagedResultDto<AdminApplicationListItemDto>? Result=null,string? Error=null);
public enum AdminApplicationMutationOutcome { Success,NotFound,InvalidStatus,InvalidTransition,FinalDecisionConflict }
public sealed record AdminApplicationMutationResult(AdminApplicationMutationOutcome Outcome,AdminApplicationDetailsDto? Application=null);
public sealed record AdminApplicationResumeDownload(Stream Content,string ContentType,string FileName);
