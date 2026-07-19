using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.Application.Interfaces;

public interface IJobRepository
{
    Task<PagedResultDto<JobListItemDto>> GetPublishedJobsAsync(
        int userId,
        JobListQueryDto query,
        CancellationToken cancellationToken = default);

    Task<JobDetailsDto?> GetPublishedJobAsync(
        int userId,
        int jobId,
        CancellationToken cancellationToken = default);
}
