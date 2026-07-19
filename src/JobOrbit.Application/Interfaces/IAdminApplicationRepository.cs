using JobOrbit.Application.DTOs.AdminApplications;
namespace JobOrbit.Application.Interfaces;
public interface IAdminApplicationRepository
{
 Task<AdminApplicationListResult> ListAsync(AdminApplicationQuery query,CancellationToken token=default);
 Task<AdminApplicationDetailsDto?> DetailsAsync(int id,CancellationToken token=default);
 Task<AdminApplicationMutationResult> UpdateStatusAsync(int adminId,int id,string status,string reason,CancellationToken token=default);
 Task<(string StoredFileName,string ContentType,string OriginalFileName)?> ResumeAsync(int id,CancellationToken token=default);
 Task<IReadOnlyList<AdminApplicationHistoryItemDto>> HistoryAsync(int id,CancellationToken token=default);
}
