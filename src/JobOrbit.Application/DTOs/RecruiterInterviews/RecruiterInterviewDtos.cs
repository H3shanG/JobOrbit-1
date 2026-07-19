using System.ComponentModel.DataAnnotations;
namespace JobOrbit.Application.DTOs.RecruiterInterviews;
public class InterviewScheduleRequest:IValidatableObject
{
 [Required]public DateTime ScheduledAt{get;init;}[Range(15,240)]public int DurationMinutes{get;init;}[Required,StringLength(200)]public string Location{get;init;}="";[Url,StringLength(500)]public string? MeetingLink{get;init;}[StringLength(2000)]public string? Notes{get;init;}
 public IEnumerable<ValidationResult>Validate(ValidationContext context){if(ScheduledAt<=DateTime.UtcNow)yield return new("Scheduled time must be in the future.",[nameof(ScheduledAt)]);}
}
public sealed class CreateInterviewRequest:InterviewScheduleRequest{[Range(1,int.MaxValue)]public int ApplicationId{get;init;}}
public sealed class UpdateInterviewRequest:InterviewScheduleRequest;
public sealed class RecruiterInterviewQuery{public string? Status{get;set;}public string? Search{get;set;}public DateTime? From{get;set;}public DateTime? To{get;set;}public int Page{get;set;}=1;public int PageSize{get;set;}=10;public string Sort{get;set;}="soonest";}
public sealed class RecruiterInterviewListItemDto{public int InterviewId{get;init;}public int ApplicationId{get;init;}public string CandidateName{get;init;}="";public string JobTitle{get;init;}="";public DateTime ScheduledAt{get;init;}public int DurationMinutes{get;init;}public string Location{get;init;}="";public string? MeetingLink{get;init;}public string Status{get;init;}="";}
public sealed class InterviewCandidateSummaryDto{public int CandidateId{get;init;}public string FullName{get;init;}="";public string Email{get;init;}="";public string? Phone{get;init;}}
public sealed class InterviewJobSummaryDto{public int JobId{get;init;}public string Title{get;init;}="";public string DepartmentName{get;init;}="";}
public sealed class InterviewApplicationSummaryDto{public int ApplicationId{get;init;}public string Status{get;init;}="";public DateTime AppliedOn{get;init;}}
public sealed class RecruiterInterviewDetailsDto{public int InterviewId{get;init;}public InterviewCandidateSummaryDto Candidate{get;init;}=new();public InterviewJobSummaryDto Job{get;init;}=new();public InterviewApplicationSummaryDto Application{get;init;}=new();public DateTime ScheduledAt{get;init;}public int DurationMinutes{get;init;}public string Location{get;init;}="";public string? MeetingLink{get;init;}public string? Notes{get;init;}public string Status{get;init;}="";public DateTime CreatedAt{get;init;}public DateTime UpdatedAt{get;init;}}
public sealed class ShortlistedApplicationDto{public int ApplicationId{get;init;}public string CandidateName{get;init;}="";public string JobTitle{get;init;}="";}
public enum RecruiterInterviewOutcome{Success,NotFound,ApplicationNotShortlisted,DuplicateActiveInterview,InvalidTransition,InterviewNotDue}
