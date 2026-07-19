using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.Application.Interfaces;

public interface IJobService
{
    Task<PagedResultDto<JobListItemDto>> GetJobsAsync(
        int userId,
        JobListQueryDto query,
        CancellationToken cancellationToken = default);

    Task<JobDetailsDto?> GetJobDetailsAsync(
        int userId,
        int jobId,
        CancellationToken cancellationToken = default);
}
