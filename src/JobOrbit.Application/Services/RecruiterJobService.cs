using JobOrbit.Application.DTOs.RecruiterJobs;
using JobOrbit.Application.Interfaces;
namespace JobOrbit.Application.Services;
public sealed class RecruiterJobService(IRecruiterJobRepository repository):IRecruiterJobService
{
 public Task<CreateRecruiterJobResult>CreateAsync(int userId,CreateRecruiterJobRequest request,CancellationToken token=default)=>repository.CreateAsync(userId,request,token);
 public Task<RecruiterJobResponse?>GetAsync(int userId,int jobId,CancellationToken token=default)=>repository.GetAsync(userId,jobId,token);
 public Task<IReadOnlyList<RecruiterReferenceDto>>DepartmentsAsync(int userId,CancellationToken token=default)=>repository.DepartmentsAsync(userId,token);
 public Task<IReadOnlyList<RecruiterReferenceDto>>SkillsAsync(CancellationToken token=default)=>repository.SkillsAsync(token);
 public Task<JobOrbit.Application.DTOs.Jobs.PagedResultDto<RecruiterJobListItemDto>>ListAsync(int u,RecruiterJobQuery q,CancellationToken t=default){q.Page=Math.Max(1,q.Page);q.PageSize=Math.Clamp(q.PageSize,1,50);return repository.ListAsync(u,q,t);}public Task<RecruiterJobDetailsDto?>DetailsAsync(int u,int j,CancellationToken t=default)=>repository.DetailsAsync(u,j,t);public Task<RecruiterJobMutationOutcome>UpdateAsync(int u,int j,UpdateRecruiterJobRequest r,CancellationToken t=default)=>repository.UpdateAsync(u,j,r,t);public Task<RecruiterJobMutationOutcome>PublishAsync(int u,int j,CancellationToken t=default)=>repository.PublishAsync(u,j,t);public Task<RecruiterJobMutationOutcome>CloseAsync(int u,int j,CancellationToken t=default)=>repository.CloseAsync(u,j,t);public Task<RecruiterJobMutationOutcome>DeleteAsync(int u,int j,CancellationToken t=default)=>repository.DeleteAsync(u,j,t);
}
