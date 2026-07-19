using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.RecruiterApplications;
using JobOrbit.Application.Interfaces;
namespace JobOrbit.Application.Services;
public sealed class RecruiterApplicationService(IRecruiterApplicationRepository repository,IFileStorageService storage):IRecruiterApplicationService
{
 public Task<PagedResultDto<RecruiterApplicationListItemDto>> ListAsync(int userId,RecruiterApplicationQuery query,CancellationToken token=default){query.Page=Math.Max(1,query.Page);query.PageSize=Math.Clamp(query.PageSize,1,50);return repository.ListAsync(userId,query,token);}
 public Task<RecruiterApplicationDetailsDto?> DetailsAsync(int userId,int applicationId,CancellationToken token=default)=>repository.DetailsAsync(userId,applicationId,token);
 public Task<RecruiterApplicationMutationOutcome> UpdateStatusAsync(int userId,int applicationId,string status,CancellationToken token=default)=>repository.UpdateStatusAsync(userId,applicationId,status,token);
 public async Task<RecruiterResumeDownloadDto?> DownloadResumeAsync(int userId,int applicationId,CancellationToken token=default){var resume=await repository.ResumeAsync(userId,applicationId,token);if(resume is null)return null;var stream=await storage.OpenReadAsync(resume.Value.StoredFileName,token);return stream is null?null:new RecruiterResumeDownloadDto(stream,resume.Value.ContentType,resume.Value.OriginalFileName);}
}
