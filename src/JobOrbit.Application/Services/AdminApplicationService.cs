using JobOrbit.Application.DTOs.AdminApplications;using JobOrbit.Application.Interfaces;
namespace JobOrbit.Application.Services;
public sealed class AdminApplicationService(IAdminApplicationRepository repository,IFileStorageService storage):IAdminApplicationService
{
 public Task<AdminApplicationListResult> ListAsync(AdminApplicationQuery q,CancellationToken t=default){q.Page=Math.Max(1,q.Page);q.PageSize=Math.Clamp(q.PageSize,1,100);return repository.ListAsync(q,t);}
 public Task<AdminApplicationDetailsDto?> DetailsAsync(int id,CancellationToken t=default)=>repository.DetailsAsync(id,t);
 public Task<AdminApplicationMutationResult> UpdateStatusAsync(int admin,int id,UpdateAdminApplicationStatusRequest r,CancellationToken t=default)=>repository.UpdateStatusAsync(admin,id,r.Status.Trim(),r.Reason.Trim(),t);
 public async Task<AdminApplicationResumeDownload?> DownloadResumeAsync(int id,CancellationToken t=default){var x=await repository.ResumeAsync(id,t);if(x is null)return null;var stream=await storage.OpenReadAsync(x.Value.StoredFileName,t);return stream is null?null:new(stream,x.Value.ContentType,x.Value.OriginalFileName);}
 public async Task<IReadOnlyList<AdminApplicationHistoryItemDto>?> HistoryAsync(int id,CancellationToken t=default)=>await repository.DetailsAsync(id,t) is null?null:await repository.HistoryAsync(id,t);
}
