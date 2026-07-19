using JobOrbit.Application.DTOs.AdminApplications;
namespace JobOrbit.Application.Interfaces;
public interface IAdminApplicationService
{
 Task<AdminApplicationListResult> ListAsync(AdminApplicationQuery query,CancellationToken token=default);
 Task<AdminApplicationDetailsDto?> DetailsAsync(int id,CancellationToken token=default);
 Task<AdminApplicationMutationResult> UpdateStatusAsync(int adminId,int id,UpdateAdminApplicationStatusRequest request,CancellationToken token=default);
 Task<AdminApplicationResumeDownload?> DownloadResumeAsync(int id,CancellationToken token=default);
 Task<IReadOnlyList<AdminApplicationHistoryItemDto>?> HistoryAsync(int id,CancellationToken token=default);
}
