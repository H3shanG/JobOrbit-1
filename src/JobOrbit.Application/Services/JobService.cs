using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.Interfaces;

namespace JobOrbit.Application.Services;

public sealed class JobService(IJobRepository jobRepository) : IJobService
{
    public Task<PagedResultDto<JobListItemDto>> GetJobsAsync(
        int userId,
        JobListQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 50);
        query.Sort = string.IsNullOrWhiteSpace(query.Sort) ? "newest" : query.Sort.Trim();
        return jobRepository.GetPublishedJobsAsync(userId, query, cancellationToken);
    }

    public Task<JobDetailsDto?> GetJobDetailsAsync(
        int userId,
        int jobId,
        CancellationToken cancellationToken = default)
    {
        return jobRepository.GetPublishedJobAsync(userId, jobId, cancellationToken);
    }
}
