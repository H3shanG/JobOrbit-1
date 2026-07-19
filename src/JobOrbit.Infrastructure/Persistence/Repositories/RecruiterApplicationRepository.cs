using System.Text.Json;using JobOrbit.Domain;using JobOrbit.Application.DTOs.Notifications;
using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.RecruiterApplications;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace JobOrbit.Infrastructure.Persistence.Repositories;
public sealed class RecruiterApplicationRepository(JobOrbitDbContext db, INotificationService notifications):IRecruiterApplicationRepository
{
 public async Task<PagedResultDto<RecruiterApplicationListItemDto>> ListAsync(int userId,RecruiterApplicationQuery query,CancellationToken token=default)
 {
  var applications=db.JobApplications.AsNoTracking().Where(x=>x.JobPosting.RecruiterProfile.UserId==userId);
  if(query.JobId.HasValue)applications=applications.Where(x=>x.JobPostingId==query.JobId);
  if(!string.IsNullOrWhiteSpace(query.Status)&&Enum.TryParse<ApplicationStatus>(query.Status,true,out var status))applications=applications.Where(x=>x.Status==status);
  if(!string.IsNullOrWhiteSpace(query.Search)){var term=query.Search.Trim();applications=applications.Where(x=>(x.CandidateProfile.User.FirstName+" "+x.CandidateProfile.User.LastName).Contains(term)||x.CandidateProfile.User.Email.Contains(term)||x.JobPosting.Title.Contains(term));}
  applications=query.Sort.ToLowerInvariant() switch{"oldest"=>applications.OrderBy(x=>x.AppliedAt),"name"=>applications.OrderBy(x=>x.CandidateProfile.User.FirstName).ThenBy(x=>x.CandidateProfile.User.LastName),_=>applications.OrderByDescending(x=>x.AppliedAt)};
  var total=await applications.CountAsync(token);var rows=await applications.Skip((query.Page-1)*query.PageSize).Take(query.PageSize).Select(x=>new{x.Id,x.CandidateProfileId,Name=x.CandidateProfile.User.FirstName+" "+x.CandidateProfile.User.LastName,x.CandidateProfile.User.Email,x.JobPostingId,JobTitle=x.JobPosting.Title,x.Status,x.AppliedAt,x.ResumeId}).ToListAsync(token);
  return new(){Items=rows.Select(x=>new RecruiterApplicationListItemDto{ApplicationId=x.Id,CandidateId=x.CandidateProfileId,CandidateName=x.Name,Email=x.Email,JobId=x.JobPostingId,JobTitle=x.JobTitle,Status=x.Status.ToString(),AppliedOn=x.AppliedAt,ResumeId=x.ResumeId}).ToList(),Page=query.Page,PageSize=query.PageSize,TotalItems=total,TotalPages=(int)Math.Ceiling(total/(double)query.PageSize)};
 }
 public async Task<RecruiterApplicationDetailsDto?> DetailsAsync(int userId,int applicationId,CancellationToken token=default)
 {
  var row=await db.JobApplications.AsNoTracking().Where(x=>x.Id==applicationId&&x.JobPosting.RecruiterProfile.UserId==userId).Select(x=>new{x.Id,x.Status,x.AppliedAt,x.UpdatedAt,x.CoverLetter,CandidateId=x.CandidateProfileId,FullName=x.CandidateProfile.User.FirstName+" "+x.CandidateProfile.User.LastName,x.CandidateProfile.User.Email,Phone=x.CandidateProfile.PhoneNumber,ProfessionalTitle=x.CandidateProfile.Headline,ProfessionalSummary=x.CandidateProfile.Summary,x.CandidateProfile.Education,x.CandidateProfile.Experience,x.CandidateProfile.LinkedInUrl,x.CandidateProfile.PortfolioUrl,JobId=x.JobPostingId,JobTitle=x.JobPosting.Title,DepartmentName=x.JobPosting.Department.Name,x.JobPosting.Location,x.JobPosting.EmploymentType,Resume=x.Resume==null?null:new{x.Resume.Id,x.Resume.DisplayName,x.Resume.OriginalFileName}}).SingleOrDefaultAsync(token);
  return row is null?null:new RecruiterApplicationDetailsDto{ApplicationId=row.Id,Status=row.Status.ToString(),AppliedOn=row.AppliedAt,UpdatedOn=row.UpdatedAt,CoverLetter=row.CoverLetter,Candidate=new(){CandidateId=row.CandidateId,FullName=row.FullName,Email=row.Email,Phone=row.Phone,ProfessionalTitle=row.ProfessionalTitle,ProfessionalSummary=row.ProfessionalSummary,Education=row.Education,Experience=row.Experience,LinkedInUrl=row.LinkedInUrl,PortfolioUrl=row.PortfolioUrl},Job=new(){JobId=row.JobId,Title=row.JobTitle,DepartmentName=row.DepartmentName,Location=row.Location,EmploymentType=row.EmploymentType},Resume=row.Resume is null?null:new(){ResumeId=row.Resume.Id,DisplayName=row.Resume.DisplayName,OriginalFileName=row.Resume.OriginalFileName}};
 }
 public async Task<RecruiterApplicationMutationOutcome> UpdateStatusAsync(int userId,int applicationId,string status,CancellationToken token=default)
 {
  if(!Enum.TryParse<ApplicationStatus>(status,true,out var next)||next is not(ApplicationStatus.UnderReview or ApplicationStatus.Shortlisted or ApplicationStatus.Rejected))return RecruiterApplicationMutationOutcome.InvalidStatus;
  var application=await db.JobApplications.Include(x=>x.JobPosting).Include(x=>x.CandidateProfile).SingleOrDefaultAsync(x=>x.Id==applicationId&&x.JobPosting.RecruiterProfile.UserId==userId,token);if(application is null)return RecruiterApplicationMutationOutcome.NotFound;
  var allowed=(application.Status,next) switch{(ApplicationStatus.Submitted,ApplicationStatus.UnderReview)=>true,(ApplicationStatus.UnderReview,ApplicationStatus.Shortlisted)=>true,(ApplicationStatus.UnderReview,ApplicationStatus.Rejected)=>true,(ApplicationStatus.Shortlisted,ApplicationStatus.Rejected)=>true,_=>false};if(!allowed)return RecruiterApplicationMutationOutcome.InvalidTransition;
  var old=application.Status;application.Status=next;application.UpdatedAt=DateTime.UtcNow;db.AuditLogs.Add(new AuditLog{UserId=userId,EntityName=nameof(JobApplication),EntityId=application.Id,Action="StatusUpdate",OldValues=JsonSerializer.Serialize(new{Status=old.ToString()}),NewValues=JsonSerializer.Serialize(new{Status=next.ToString()})});await db.SaveChangesAsync(token);await notifications.CreateAsync(new(application.CandidateProfile.UserId,NotificationTypes.ApplicationStatusChanged,"Application status updated",$"Your application for {application.JobPosting.Title} is now {next}.",nameof(JobApplication),application.Id,$"/candidate/applications/{application.Id}",EventKey:$"application:{application.Id}:status:{next}"),token);if(next==ApplicationStatus.Shortlisted){var managerIds=await db.HiringManagerProfiles.AsNoTracking().Where(x=>x.OrganizationId==application.JobPosting.OrganizationId&&(!x.DepartmentId.HasValue||x.DepartmentId==application.JobPosting.DepartmentId)&&x.User.IsActive).Select(x=>x.UserId).ToListAsync(token);await notifications.CreateManyAsync(managerIds.Select(managerId=>new NotificationCreateRequest(managerId,NotificationTypes.CandidateReadyForReview,"Candidate ready for review",$"A candidate for {application.JobPosting.Title} is ready for review.",nameof(JobApplication),application.Id,$"/manager/candidates/{application.Id}",EventKey:$"application:{application.Id}:manager:{managerId}:ready")),token);}return RecruiterApplicationMutationOutcome.Success;
 }
 public async Task<(string StoredFileName,string ContentType,string OriginalFileName)?> ResumeAsync(int userId,int applicationId,CancellationToken token=default)
 {
  var row=await db.JobApplications.AsNoTracking().Where(x=>x.Id==applicationId&&x.JobPosting.RecruiterProfile.UserId==userId&&x.Resume!=null).Select(x=>new{x.Resume!.StoredFileName,x.Resume.ContentType,x.Resume.OriginalFileName}).SingleOrDefaultAsync(token);return row is null?null:(row.StoredFileName,row.ContentType,row.OriginalFileName);
 }
}
