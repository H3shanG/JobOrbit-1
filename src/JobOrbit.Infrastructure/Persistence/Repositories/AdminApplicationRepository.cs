using System.Text.Json;
using JobOrbit.Application.DTOs.AdminApplications;
using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class AdminApplicationRepository(JobOrbitDbContext db):IAdminApplicationRepository
{
 public async Task<AdminApplicationListResult> ListAsync(AdminApplicationQuery q,CancellationToken t=default)
 {
  if(q.From.HasValue&&q.To.HasValue&&q.From>q.To)return new(false,null,"Invalid date range");
  if(!string.IsNullOrWhiteSpace(q.Status)&&!Enum.TryParse<ApplicationStatus>(q.Status,true,out _))return new(false,null,"Invalid application status");
  if(!string.IsNullOrWhiteSpace(q.Decision)&&!q.Decision.Equals("Pending",StringComparison.OrdinalIgnoreCase)&&!Enum.TryParse<ManagerHiringDecision>(q.Decision,true,out _))return new(false,null,"Invalid hiring decision");
  if(q.JobId.HasValue&&!await db.JobPostings.AnyAsync(x=>x.Id==q.JobId,t))return new(false,null,"Job not found");
  if(q.OrganizationId.HasValue&&!await db.Organizations.AnyAsync(x=>x.Id==q.OrganizationId,t))return new(false,null,"Organization not found");
  if(q.DepartmentId.HasValue&&!await db.Departments.AnyAsync(x=>x.Id==q.DepartmentId,t))return new(false,null,"Department not found");
  if(q.RecruiterId.HasValue&&!await db.RecruiterProfiles.AnyAsync(x=>x.UserId==q.RecruiterId,t))return new(false,null,"Recruiter not found");
  if(q.CandidateId.HasValue&&!await db.CandidateProfiles.AnyAsync(x=>x.Id==q.CandidateId,t))return new(false,null,"Candidate not found");
  var rows=db.JobApplications.AsNoTracking();
  if(!string.IsNullOrWhiteSpace(q.Search)){var s=q.Search.Trim();rows=rows.Where(x=>(x.CandidateProfile.User.FirstName+" "+x.CandidateProfile.User.LastName).Contains(s)||x.CandidateProfile.User.Email.Contains(s)||x.JobPosting.Title.Contains(s)||x.JobPosting.Organization.Name.Contains(s)||(x.JobPosting.RecruiterProfile.User.FirstName+" "+x.JobPosting.RecruiterProfile.User.LastName).Contains(s));}
  if(!string.IsNullOrWhiteSpace(q.Status)){var s=Enum.Parse<ApplicationStatus>(q.Status,true);rows=rows.Where(x=>x.Status==s);}if(q.JobId.HasValue)rows=rows.Where(x=>x.JobPostingId==q.JobId);if(q.OrganizationId.HasValue)rows=rows.Where(x=>x.JobPosting.OrganizationId==q.OrganizationId);if(q.DepartmentId.HasValue)rows=rows.Where(x=>x.JobPosting.DepartmentId==q.DepartmentId);if(q.RecruiterId.HasValue)rows=rows.Where(x=>x.JobPosting.RecruiterProfile.UserId==q.RecruiterId);if(q.CandidateId.HasValue)rows=rows.Where(x=>x.CandidateProfileId==q.CandidateId);
  if(!string.IsNullOrWhiteSpace(q.Decision)){if(q.Decision.Equals("Pending",StringComparison.OrdinalIgnoreCase))rows=rows.Where(x=>x.HiringDecision==null);else{var d=Enum.Parse<ManagerHiringDecision>(q.Decision,true);rows=rows.Where(x=>x.HiringDecision!=null&&x.HiringDecision.Decision==d);}}if(q.From.HasValue)rows=rows.Where(x=>x.AppliedAt>=q.From.Value);if(q.To.HasValue)rows=rows.Where(x=>x.AppliedAt<=q.To.Value);
  rows=q.Sort.ToLowerInvariant()switch{"oldest"=>rows.OrderBy(x=>x.AppliedAt),"candidate"=>rows.OrderBy(x=>x.CandidateProfile.User.FirstName).ThenBy(x=>x.CandidateProfile.User.LastName),"updated"=>rows.OrderByDescending(x=>x.UpdatedAt),_=>rows.OrderByDescending(x=>x.AppliedAt)};
  var total=await rows.CountAsync(t);var items=await rows.Skip((q.Page-1)*q.PageSize).Take(q.PageSize).Select(x=>new AdminApplicationListItemDto(x.Id,x.CandidateProfileId,x.CandidateProfile.User.FirstName+" "+x.CandidateProfile.User.LastName,x.CandidateProfile.User.Email,x.JobPostingId,x.JobPosting.Title,x.JobPosting.OrganizationId,x.JobPosting.Organization.Name,x.JobPosting.DepartmentId,x.JobPosting.Department.Name,x.JobPosting.RecruiterProfile.UserId,x.JobPosting.RecruiterProfile.User.FirstName+" "+x.JobPosting.RecruiterProfile.User.LastName,x.Status.ToString(),x.Interviews.OrderByDescending(i=>i.ScheduledAt).Select(i=>i.Status.ToString()).FirstOrDefault(),x.CandidateEvaluations.Any()?"Completed":"Pending",x.HiringDecision==null?"Pending":x.HiringDecision.Decision.ToString(),null,x.AppliedAt,x.UpdatedAt,x.ResumeId)).ToListAsync(t);
  return new(true,new PagedResultDto<AdminApplicationListItemDto>{Items=items,Page=q.Page,PageSize=q.PageSize,TotalItems=total,TotalPages=(int)Math.Ceiling(total/(double)q.PageSize)});
 }
 public async Task<AdminApplicationDetailsDto?> DetailsAsync(int id,CancellationToken t=default)
 {
  var x=await db.JobApplications.AsNoTracking().Where(a=>a.Id==id).Select(a=>new{a.Id,a.Status,a.AppliedAt,a.UpdatedAt,a.CoverLetter,Candidate=new AdminApplicationCandidateSummaryDto(a.CandidateProfileId,a.CandidateProfile.User.FirstName+" "+a.CandidateProfile.User.LastName,a.CandidateProfile.User.Email,a.CandidateProfile.PhoneNumber,a.CandidateProfile.Headline,a.CandidateProfile.CandidateSkills.OrderBy(s=>s.Skill.Name).Select(s=>s.Skill.Name).ToList()),Job=new AdminApplicationJobSummaryDto(a.JobPostingId,a.JobPosting.Title,a.JobPosting.Status.ToString(),a.JobPosting.Organization.Name,a.JobPosting.Department.Name,a.JobPosting.RecruiterProfile.User.FirstName+" "+a.JobPosting.RecruiterProfile.User.LastName),Resume=a.Resume==null?null:new AdminApplicationResumeSummaryDto(a.Resume.Id,a.Resume.DisplayName,a.Resume.OriginalFileName),Interviews=a.Interviews.OrderByDescending(i=>i.ScheduledAt).Select(i=>new AdminApplicationInterviewSummaryDto(i.Id,i.ScheduledAt,i.DurationMinutes,i.Location,i.Status.ToString())).ToList(),Evaluations=a.CandidateEvaluations.OrderByDescending(e=>e.CreatedAt).Select(e=>new AdminApplicationEvaluationSummaryDto(e.Id,e.EvaluatorUser!=null?e.EvaluatorUser.FirstName+" "+e.EvaluatorUser.LastName:e.RecruiterProfile!=null?e.RecruiterProfile.User.FirstName+" "+e.RecruiterProfile.User.LastName:"Unknown",e.OverallScore,e.Recommendation==null?null:e.Recommendation.Value.ToString(),e.CreatedAt)).ToList(),Decision=a.HiringDecision==null?null:new AdminApplicationDecisionSummaryDto(a.HiringDecision.Decision.ToString(),a.HiringDecision.DecidedByUser.FirstName+" "+a.HiringDecision.DecidedByUser.LastName,a.HiringDecision.DecidedAt)}).SingleOrDefaultAsync(t);
  return x is null?null:new(x.Id,x.Status.ToString(),x.AppliedAt,x.UpdatedAt,x.CoverLetter,null,x.Candidate,x.Job,x.Resume,x.Interviews,x.Evaluations,x.Decision);
 }
 public async Task<AdminApplicationMutationResult> UpdateStatusAsync(int admin,int id,string status,string reason,CancellationToken t=default)
 {
  if(!Enum.TryParse<ApplicationStatus>(status,true,out var next))return new(AdminApplicationMutationOutcome.InvalidStatus);var app=await db.JobApplications.Include(x=>x.HiringDecision).SingleOrDefaultAsync(x=>x.Id==id,t);if(app is null)return new(AdminApplicationMutationOutcome.NotFound);if(app.Status==next)return new(AdminApplicationMutationOutcome.InvalidTransition);
  if(app.HiringDecision?.Decision is ManagerHiringDecision.Hire or ManagerHiringDecision.Reject)return new(AdminApplicationMutationOutcome.FinalDecisionConflict);
  var allowed=(app.Status,next)switch{(ApplicationStatus.Submitted,ApplicationStatus.UnderReview)=>true,(ApplicationStatus.UnderReview,ApplicationStatus.Shortlisted)=>true,(ApplicationStatus.UnderReview,ApplicationStatus.Rejected)=>true,(ApplicationStatus.Shortlisted,ApplicationStatus.Rejected)=>true,(ApplicationStatus.Interviewing,ApplicationStatus.Rejected)=>true,_=>false};if(!allowed)return new(AdminApplicationMutationOutcome.InvalidTransition);
  await using var tx=db.Database.IsRelational()?await db.Database.BeginTransactionAsync(t):null;var old=app.Status;app.Status=next;app.UpdatedAt=DateTime.UtcNow;db.AuditLogs.Add(new AuditLog{UserId=admin,EntityName=nameof(JobApplication),EntityId=id,Action="AdminOverrideApplicationStatus",OldValues=JsonSerializer.Serialize(new{Status=old.ToString()}),NewValues=JsonSerializer.Serialize(new{Status=next.ToString(),Reason=reason,ApplicationId=id})});await db.SaveChangesAsync(t);if(tx is not null)await tx.CommitAsync(t);return new(AdminApplicationMutationOutcome.Success,await DetailsAsync(id,t));
 }
 public async Task<(string StoredFileName,string ContentType,string OriginalFileName)?> ResumeAsync(int id,CancellationToken t=default){var x=await db.JobApplications.AsNoTracking().Where(a=>a.Id==id&&a.Resume!=null).Select(a=>new{a.Resume!.StoredFileName,a.Resume.ContentType,a.Resume.OriginalFileName}).SingleOrDefaultAsync(t);return x is null?null:(x.StoredFileName,x.ContentType,x.OriginalFileName);}
 public async Task<IReadOnlyList<AdminApplicationHistoryItemDto>> HistoryAsync(int id,CancellationToken t=default)
 {
  var events=new List<AdminApplicationHistoryItemDto>();var app=await db.JobApplications.AsNoTracking().Where(x=>x.Id==id).Select(x=>new{x.AppliedAt}).SingleAsync(t);events.Add(new("Applied","Application submitted.",null,"Candidate",app.AppliedAt));
  events.AddRange(await db.AuditLogs.AsNoTracking().Where(x=>(x.EntityName==nameof(JobApplication)&&x.EntityId==id)||(x.EntityName==nameof(ApplicationHiringDecision)&&x.EntityId==id)).Select(x=>new AdminApplicationHistoryItemDto(x.Action,x.Action=="AdminOverrideApplicationStatus"?"Application status corrected by an administrator.":x.Action,x.User==null?null:x.User.FirstName+" "+x.User.LastName,x.User==null?null:x.User.Role.ToString(),x.CreatedAt)).ToListAsync(t));
  events.AddRange(await db.Interviews.AsNoTracking().Where(x=>x.JobApplicationId==id).Select(x=>new AdminApplicationHistoryItemDto("Interview","Interview "+x.Status.ToString()+".",null,null,x.CreatedAt)).ToListAsync(t));events.AddRange(await db.CandidateEvaluations.AsNoTracking().Where(x=>x.JobApplicationId==id).Select(x=>new AdminApplicationHistoryItemDto("Evaluation","Candidate evaluation submitted.",x.EvaluatorUser==null?null:x.EvaluatorUser.FirstName+" "+x.EvaluatorUser.LastName,x.EvaluatorUser==null?null:x.EvaluatorUser.Role.ToString(),x.CreatedAt)).ToListAsync(t));return events.OrderBy(x=>x.OccurredAt).ToList();
 }
}
