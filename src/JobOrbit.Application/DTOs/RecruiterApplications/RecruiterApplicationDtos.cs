using System.ComponentModel.DataAnnotations;
using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.Application.DTOs.RecruiterApplications;

public sealed class RecruiterApplicationQuery { public int? JobId{get;set;} public string? Status{get;set;} public string? Search{get;set;} public int Page{get;set;}=1; public int PageSize{get;set;}=10; public string Sort{get;set;}="newest"; }
public sealed class RecruiterApplicationListItemDto { public int ApplicationId{get;init;} public int CandidateId{get;init;} public string CandidateName{get;init;}=""; public string Email{get;init;}=""; public int JobId{get;init;} public string JobTitle{get;init;}=""; public string Status{get;init;}=""; public DateTime AppliedOn{get;init;} public int? ResumeId{get;init;} }
public sealed class RecruiterCandidateSummaryDto { public int CandidateId{get;init;} public string FullName{get;init;}=""; public string Email{get;init;}=""; public string? Phone{get;init;} public string? ProfessionalTitle{get;init;} public string? ProfessionalSummary{get;init;} public string? Education{get;init;} public string? Experience{get;init;} public string? LinkedInUrl{get;init;} public string? PortfolioUrl{get;init;} }
public sealed class RecruiterApplicationJobSummaryDto { public int JobId{get;init;} public string Title{get;init;}=""; public string DepartmentName{get;init;}=""; public string Location{get;init;}=""; public string EmploymentType{get;init;}=""; }
public sealed class RecruiterApplicationResumeDto { public int ResumeId{get;init;} public string DisplayName{get;init;}=""; public string OriginalFileName{get;init;}=""; }
public sealed class RecruiterApplicationDetailsDto { public int ApplicationId{get;init;} public string Status{get;init;}=""; public DateTime AppliedOn{get;init;} public DateTime UpdatedOn{get;init;} public string? CoverLetter{get;init;} public RecruiterCandidateSummaryDto Candidate{get;init;}=new(); public RecruiterApplicationJobSummaryDto Job{get;init;}=new(); public RecruiterApplicationResumeDto? Resume{get;init;} }
public sealed class UpdateApplicationStatusRequest { [Required] public string Status{get;init;}=""; }
public sealed record RecruiterResumeDownloadDto(Stream Content,string ContentType,string OriginalFileName);
public enum RecruiterApplicationMutationOutcome { Success,NotFound,InvalidStatus,InvalidTransition }
