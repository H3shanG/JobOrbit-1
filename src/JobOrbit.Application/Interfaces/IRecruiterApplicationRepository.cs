using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.RecruiterApplications;
namespace JobOrbit.Application.Interfaces;
public interface IRecruiterApplicationRepository
{
 Task<PagedResultDto<RecruiterApplicationListItemDto>> ListAsync(int userId,RecruiterApplicationQuery query,CancellationToken token=default);
 Task<RecruiterApplicationDetailsDto?> DetailsAsync(int userId,int applicationId,CancellationToken token=default);
 Task<RecruiterApplicationMutationOutcome> UpdateStatusAsync(int userId,int applicationId,string status,CancellationToken token=default);
 Task<(string StoredFileName,string ContentType,string OriginalFileName)?> ResumeAsync(int userId,int applicationId,CancellationToken token=default);
}
