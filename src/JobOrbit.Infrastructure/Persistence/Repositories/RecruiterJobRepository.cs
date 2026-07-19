using System.Text.Json;
using JobOrbit.Application.DTOs.RecruiterJobs;
using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace JobOrbit.Infrastructure.Persistence.Repositories;
public sealed class RecruiterJobRepository(JobOrbitDbContext db,ISystemSettingsProvider systemSettings):IRecruiterJobRepository
{
 public async Task<CreateRecruiterJobResult>CreateAsync(int userId,CreateRecruiterJobRequest request,CancellationToken token=default)
 {
  var profile=await db.RecruiterProfiles.SingleOrDefaultAsync(x=>x.UserId==userId,token);if(profile is null)return new(CreateRecruiterJobOutcome.RecruiterProfileMissing);
  if(!await db.Departments.AnyAsync(x=>x.Id==request.DepartmentId&&x.OrganizationId==profile.OrganizationId&&x.IsActive&&x.Organization.IsActive,token))return new(CreateRecruiterJobOutcome.InvalidDepartment);
  var skillIds=request.SkillIds.Distinct().ToList();if(await db.Skills.CountAsync(x=>skillIds.Contains(x.Id),token)!=skillIds.Count)return new(CreateRecruiterJobOutcome.InvalidSkills);
  var now=DateTime.UtcNow;var defaults=await systemSettings.GetAsync(token);var closing=request.ClosingDate==default?now.AddDays(defaults.Recruitment.DefaultJobClosingDays):request.ClosingDate.ToUniversalTime();var job=new JobPosting{OrganizationId=profile.OrganizationId,DepartmentId=request.DepartmentId,RecruiterProfileId=profile.Id,Title=request.Title.Trim(),Location=request.Location.Trim(),EmploymentType=request.EmploymentType.Trim(),Description=request.Description.Trim(),Responsibilities=Clean(request.Responsibilities),Requirements=Clean(request.Requirements),SalaryMinimum=request.MinimumSalary,SalaryMaximum=request.MaximumSalary,ClosingAt=closing,Status=request.PublishNow?JobStatus.Published:JobStatus.Draft,PublishedAt=request.PublishNow?now:null};
  foreach(var skillId in skillIds)job.JobSkills.Add(new JobSkill{SkillId=skillId,IsRequired=true});db.JobPostings.Add(job);await db.SaveChangesAsync(token);
  db.AuditLogs.Add(new AuditLog{UserId=userId,EntityName=nameof(JobPosting),EntityId=job.Id,Action="Create",NewValues=JsonSerializer.Serialize(new{job.Title,job.Status})});await db.SaveChangesAsync(token);
  return new(CreateRecruiterJobOutcome.Created,Map(job));
 }
 public async Task<RecruiterJobResponse?>GetAsync(int userId,int jobId,CancellationToken token=default){var row=await db.JobPostings.AsNoTracking().Where(x=>x.Id==jobId&&x.RecruiterProfile.UserId==userId).Select(x=>new{x.Id,x.Title,x.Status,x.CreatedAt}).SingleOrDefaultAsync(token);return row is null?null:new RecruiterJobResponse{JobId=row.Id,Title=row.Title,Status=row.Status.ToString(),CreatedAt=row.CreatedAt};}
 public async Task<PagedResultDto<RecruiterJobListItemDto>> ListAsync(int userId,RecruiterJobQuery query,CancellationToken token=default)
 {
  var jobs=db.JobPostings.AsNoTracking().Where(x=>x.RecruiterProfile.UserId==userId);
  if(!string.IsNullOrWhiteSpace(query.Search)){var term=query.Search.Trim();jobs=jobs.Where(x=>x.Title.Contains(term)||x.Department.Name.Contains(term)||x.Location.Contains(term)||x.EmploymentType.Contains(term));}
  if(!string.IsNullOrWhiteSpace(query.Status)&&Enum.TryParse<JobStatus>(query.Status,true,out var status))jobs=jobs.Where(x=>x.Status==status);
  jobs=query.Sort.ToLowerInvariant() switch{"oldest"=>jobs.OrderBy(x=>x.CreatedAt),"closing"=>jobs.OrderBy(x=>x.ClosingAt),_=>jobs.OrderByDescending(x=>x.CreatedAt)};
  var total=await jobs.CountAsync(token);
  var rows=await jobs.Skip((query.Page-1)*query.PageSize).Take(query.PageSize).Select(x=>new{x.Id,x.Title,DepartmentName=x.Department.Name,x.Location,x.EmploymentType,x.Status,ApplicationCount=x.JobApplications.Count,x.CreatedAt,x.ClosingAt}).ToListAsync(token);
  return new PagedResultDto<RecruiterJobListItemDto>{Items=rows.Select(x=>new RecruiterJobListItemDto{JobId=x.Id,Title=x.Title,DepartmentName=x.DepartmentName,Location=x.Location,EmploymentType=x.EmploymentType,Status=x.Status.ToString(),ApplicationCount=x.ApplicationCount,CreatedAt=x.CreatedAt,ClosingDate=x.ClosingAt}).ToList(),Page=query.Page,PageSize=query.PageSize,TotalItems=total,TotalPages=(int)Math.Ceiling(total/(double)query.PageSize)};
 }
 public async Task<RecruiterJobDetailsDto?> DetailsAsync(int userId,int jobId,CancellationToken token=default)
 {
  var row=await db.JobPostings.AsNoTracking().Where(x=>x.Id==jobId&&x.RecruiterProfile.UserId==userId).Select(x=>new{x.Id,x.Title,x.DepartmentId,DepartmentName=x.Department.Name,x.Location,x.EmploymentType,x.Description,x.Responsibilities,x.Requirements,x.SalaryMinimum,x.SalaryMaximum,x.ClosingAt,x.Status,ApplicationCount=x.JobApplications.Count,x.CreatedAt,x.UpdatedAt,Skills=x.JobSkills.OrderBy(s=>s.Skill.Name).Select(s=>new RecruiterReferenceDto{Id=s.SkillId,Name=s.Skill.Name}).ToList()}).SingleOrDefaultAsync(token);
  return row is null?null:new RecruiterJobDetailsDto{JobId=row.Id,Title=row.Title,DepartmentId=row.DepartmentId,DepartmentName=row.DepartmentName,Location=row.Location,EmploymentType=row.EmploymentType,Description=row.Description,Responsibilities=row.Responsibilities,Requirements=row.Requirements,MinimumSalary=row.SalaryMinimum,MaximumSalary=row.SalaryMaximum,ClosingDate=row.ClosingAt,Status=row.Status.ToString(),ApplicationCount=row.ApplicationCount,CreatedAt=row.CreatedAt,UpdatedAt=row.UpdatedAt,Skills=row.Skills};
 }
 public async Task<RecruiterJobMutationOutcome> UpdateAsync(int userId,int jobId,UpdateRecruiterJobRequest request,CancellationToken token=default)
 {
  var profile=await db.RecruiterProfiles.SingleOrDefaultAsync(x=>x.UserId==userId,token);if(profile is null)return RecruiterJobMutationOutcome.NotFound;
  var job=await db.JobPostings.Include(x=>x.JobSkills).SingleOrDefaultAsync(x=>x.Id==jobId&&x.RecruiterProfileId==profile.Id,token);if(job is null)return RecruiterJobMutationOutcome.NotFound;
  if(!await db.Departments.AnyAsync(x=>x.Id==request.DepartmentId&&x.OrganizationId==profile.OrganizationId&&x.IsActive&&x.Organization.IsActive,token))return RecruiterJobMutationOutcome.InvalidDepartment;
  var skillIds=request.SkillIds.Distinct().ToList();if(await db.Skills.CountAsync(x=>skillIds.Contains(x.Id),token)!=skillIds.Count)return RecruiterJobMutationOutcome.InvalidSkills;
  var old=JsonSerializer.Serialize(new{job.Title,job.Status});job.Title=request.Title.Trim();job.DepartmentId=request.DepartmentId;job.Location=request.Location.Trim();job.EmploymentType=request.EmploymentType.Trim();job.Description=request.Description.Trim();job.Responsibilities=Clean(request.Responsibilities);job.Requirements=Clean(request.Requirements);job.SalaryMinimum=request.MinimumSalary;job.SalaryMaximum=request.MaximumSalary;job.ClosingAt=request.ClosingDate.ToUniversalTime();
  db.JobSkills.RemoveRange(job.JobSkills.Where(x=>!skillIds.Contains(x.SkillId)));foreach(var id in skillIds.Where(id=>job.JobSkills.All(x=>x.SkillId!=id)))job.JobSkills.Add(new JobSkill{SkillId=id,IsRequired=true});
  db.AuditLogs.Add(new AuditLog{UserId=userId,EntityName=nameof(JobPosting),EntityId=job.Id,Action="Update",OldValues=old,NewValues=JsonSerializer.Serialize(new{job.Title,job.Status})});await db.SaveChangesAsync(token);return RecruiterJobMutationOutcome.Success;
 }
 public Task<RecruiterJobMutationOutcome> PublishAsync(int userId,int jobId,CancellationToken token=default)=>TransitionAsync(userId,jobId,JobStatus.Draft,JobStatus.Published,"Publish",token);
 public Task<RecruiterJobMutationOutcome> CloseAsync(int userId,int jobId,CancellationToken token=default)=>TransitionAsync(userId,jobId,JobStatus.Published,JobStatus.Closed,"Close",token);
 private async Task<RecruiterJobMutationOutcome> TransitionAsync(int userId,int jobId,JobStatus expected,JobStatus next,string action,CancellationToken token)
 {
  var job=await db.JobPostings.SingleOrDefaultAsync(x=>x.Id==jobId&&x.RecruiterProfile.UserId==userId,token);if(job is null)return RecruiterJobMutationOutcome.NotFound;if(job.Status!=expected)return RecruiterJobMutationOutcome.InvalidTransition;if(next==JobStatus.Published&&(!job.ClosingAt.HasValue||job.ClosingAt<=DateTime.UtcNow))return RecruiterJobMutationOutcome.InvalidTransition;
  var old=job.Status;job.Status=next;if(next==JobStatus.Published)job.PublishedAt=DateTime.UtcNow;db.AuditLogs.Add(new AuditLog{UserId=userId,EntityName=nameof(JobPosting),EntityId=job.Id,Action=action,OldValues=old.ToString(),NewValues=next.ToString()});await db.SaveChangesAsync(token);return RecruiterJobMutationOutcome.Success;
 }
 public async Task<RecruiterJobMutationOutcome> DeleteAsync(int userId,int jobId,CancellationToken token=default)
 {
  var job=await db.JobPostings.Include(x=>x.JobApplications).Include(x=>x.JobSkills).SingleOrDefaultAsync(x=>x.Id==jobId&&x.RecruiterProfile.UserId==userId,token);if(job is null)return RecruiterJobMutationOutcome.NotFound;if(job.JobApplications.Count!=0)return RecruiterJobMutationOutcome.HasApplications;
  db.JobSkills.RemoveRange(job.JobSkills);db.JobPostings.Remove(job);db.AuditLogs.Add(new AuditLog{UserId=userId,EntityName=nameof(JobPosting),EntityId=job.Id,Action="Delete",OldValues=JsonSerializer.Serialize(new{job.Title,job.Status})});await db.SaveChangesAsync(token);return RecruiterJobMutationOutcome.Success;
 }
 public async Task<IReadOnlyList<RecruiterReferenceDto>>DepartmentsAsync(int userId,CancellationToken token=default)=>await db.Departments.AsNoTracking().Where(x=>x.Organization.Recruiters.Any(r=>r.UserId==userId)).OrderBy(x=>x.Name).Select(x=>new RecruiterReferenceDto{Id=x.Id,Name=x.Name}).ToListAsync(token);
 public async Task<IReadOnlyList<RecruiterReferenceDto>>SkillsAsync(CancellationToken token=default)=>await db.Skills.AsNoTracking().OrderBy(x=>x.Name).Select(x=>new RecruiterReferenceDto{Id=x.Id,Name=x.Name}).ToListAsync(token);
 private static string?Clean(string?x)=>string.IsNullOrWhiteSpace(x)?null:x.Trim();private static RecruiterJobResponse Map(JobPosting x)=>new(){JobId=x.Id,Title=x.Title,Status=x.Status.ToString(),CreatedAt=x.CreatedAt};
}
