using JobOrbit.Application.DTOs.Applications;
using JobOrbit.Application.Interfaces;
using JobOrbit.Application.Services;

namespace JobOrbit.Tests;

public sealed class JobApplicationServiceTests
{
    [Fact]
    public async Task ApplyAsync_UsesAuthenticatedUserAndTrimsCoverLetter()
    {
        var repository = new FakeRepository();
        var service = new JobApplicationService(repository);

        var result = await service.ApplyAsync(17, 23, new CreateJobApplicationRequest
        {
            CoverLetter = "  I am very interested in this opportunity.  "
        });

        Assert.Equal(CreateApplicationOutcome.Created, result.Outcome);
        Assert.Equal(17, repository.UserId);
        Assert.Equal(23, repository.JobId);
        Assert.Equal("I am very interested in this opportunity.", repository.CoverLetter);
    }

    private sealed class FakeRepository : IJobApplicationRepository
    {
        public int UserId { get; private set; }
        public int JobId { get; private set; }
        public string CoverLetter { get; private set; } = string.Empty;

        public Task<CreateApplicationResult> CreateAsync(
            int userId,
            int jobId,
            string coverLetter,
            int? resumeId,
            CancellationToken cancellationToken = default)
        {
            UserId = userId;
            JobId = jobId;
            CoverLetter = coverLetter;
            return Task.FromResult(new CreateApplicationResult(
                CreateApplicationOutcome.Created,
                new JobApplicationResponse()));
        }
    }
}
