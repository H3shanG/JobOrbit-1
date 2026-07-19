using JobOrbit.Application.DTOs.Applications;
using JobOrbit.Application.Interfaces;
using JobOrbit.Application.Services;

namespace JobOrbit.Tests;

public sealed class CandidateApplicationServiceTests
{
    [Fact]
    public async Task GetApplicationsAsync_NormalizesPagingAndUsesCurrentUser()
    {
        var repository = new FakeRepository();
        var service = new CandidateApplicationService(repository);
        var query = new CandidateApplicationQueryDto { Page = 0, PageSize = 100, Sort = " " };

        await service.GetApplicationsAsync(41, query);

        Assert.Equal(41, repository.UserId);
        Assert.Equal(1, query.Page);
        Assert.Equal(50, query.PageSize);
        Assert.Equal("newest", query.Sort);
    }

    [Fact]
    public async Task GetApplicationAsync_UsesCurrentUserAndApplicationId()
    {
        var repository = new FakeRepository();
        var service = new CandidateApplicationService(repository);
        await service.GetApplicationAsync(41, 82);
        Assert.Equal(41, repository.UserId);
        Assert.Equal(82, repository.ApplicationId);
    }

    private sealed class FakeRepository : ICandidateApplicationRepository
    {
        public int UserId { get; private set; }
        public int ApplicationId { get; private set; }
        public Task<CandidateApplicationsPageDto> GetApplicationsAsync(int userId, CandidateApplicationQueryDto query, CancellationToken cancellationToken = default)
        {
            UserId = userId;
            return Task.FromResult(new CandidateApplicationsPageDto());
        }

        public Task<CandidateApplicationDetailsDto?> GetApplicationAsync(int userId, int applicationId, CancellationToken cancellationToken = default)
        {
            UserId = userId;
            ApplicationId = applicationId;
            return Task.FromResult<CandidateApplicationDetailsDto?>(new CandidateApplicationDetailsDto());
        }
    }
}
