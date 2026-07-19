using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.Interfaces;
using JobOrbit.Application.Services;

namespace JobOrbit.Tests;

public sealed class JobServiceTests
{
    [Fact]
    public async Task GetJobsAsync_NormalizesPagingAndPassesAuthenticatedUser()
    {
        var repository = new FakeJobRepository();
        var service = new JobService(repository);
        var query = new JobListQueryDto { Page = 0, PageSize = 100, Sort = " " };

        await service.GetJobsAsync(27, query);

        Assert.Equal(27, repository.UserId);
        Assert.Equal(1, query.Page);
        Assert.Equal(50, query.PageSize);
        Assert.Equal("newest", query.Sort);
    }

    [Fact]
    public async Task GetJobDetailsAsync_PassesCurrentUserAndJobId()
    {
        var repository = new FakeJobRepository();
        var service = new JobService(repository);

        await service.GetJobDetailsAsync(27, 91);

        Assert.Equal(27, repository.UserId);
        Assert.Equal(91, repository.JobId);
    }

    private sealed class FakeJobRepository : IJobRepository
    {
        public int UserId { get; private set; }
        public int JobId { get; private set; }

        public Task<PagedResultDto<JobListItemDto>> GetPublishedJobsAsync(
            int userId,
            JobListQueryDto query,
            CancellationToken cancellationToken = default)
        {
            UserId = userId;
            return Task.FromResult(new PagedResultDto<JobListItemDto>());
        }

        public Task<JobDetailsDto?> GetPublishedJobAsync(int userId, int jobId, CancellationToken cancellationToken = default)
        {
            UserId = userId;
            JobId = jobId;
            return Task.FromResult<JobDetailsDto?>(new JobDetailsDto { JobId = jobId });
        }
    }
}
