using JobOrbit.Application.DTOs.RecruiterJobs;
using JobOrbit.Application.DTOs.Jobs;
namespace JobOrbit.Application.Interfaces;
public interface IRecruiterJobRepository
{
 Task<CreateRecruiterJobResult> CreateAsync(int userId,CreateRecruiterJobRequest request,CancellationToken token=default);
 Task<RecruiterJobResponse?> GetAsync(int userId,int jobId,CancellationToken token=default);
 Task<IReadOnlyList<RecruiterReferenceDto>> DepartmentsAsync(int userId,CancellationToken token=default);
 Task<IReadOnlyList<RecruiterReferenceDto>> SkillsAsync(CancellationToken token=default);
 Task<PagedResultDto<RecruiterJobListItemDto>> ListAsync(int userId,RecruiterJobQuery query,CancellationToken token=default);
 Task<RecruiterJobDetailsDto?> DetailsAsync(int userId,int jobId,CancellationToken token=default);
 Task<RecruiterJobMutationOutcome> UpdateAsync(int userId,int jobId,UpdateRecruiterJobRequest request,CancellationToken token=default);
 Task<RecruiterJobMutationOutcome> PublishAsync(int userId,int jobId,CancellationToken token=default);
 Task<RecruiterJobMutationOutcome> CloseAsync(int userId,int jobId,CancellationToken token=default);
 Task<RecruiterJobMutationOutcome> DeleteAsync(int userId,int jobId,CancellationToken token=default);
}
