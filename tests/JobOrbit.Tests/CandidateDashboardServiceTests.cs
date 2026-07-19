using System.Security.Claims;
using JobOrbit.API.Controllers;
using JobOrbit.Application.DTOs.Dashboard;
using JobOrbit.Application.Interfaces;
using JobOrbit.Application.Services;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.Tests;

public sealed class CandidateDashboardServiceTests
{
    [Fact]
    public async Task GetStatsAsync_ReturnsRepositoryCountsForCurrentUser()
    {
        var expected = new CandidateDashboardStatsDto
        {
            JobsApplied = 6,
            Interviews = 2,
            Shortlisted = 1,
            Pending = 3
        };
        var repository = new FakeCandidateDashboardRepository(expected);
        var service = new CandidateDashboardService(repository);

        var result = await service.GetStatsAsync(42);

        Assert.Same(expected, result);
        Assert.Equal(42, repository.RequestedUserId);
    }

    [Fact]
    public async Task Controller_UsesUserIdClaimAndReturnsStats()
    {
        var stats = new CandidateDashboardStatsDto();
        var service = new FakeCandidateDashboardService(stats);
        var controller = new DashboardController(
            service,
            new FakeRecruiterDashboardService(),
            new FakeHiringManagerDashboardService())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim("UserId", "17")],
                        "Test"))
                }
            }
        };

        var result = await controller.GetCandidateStats(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(stats, ok.Value);
        Assert.Equal(17, service.RequestedUserId);
    }

    [Fact]
    public void Endpoint_RequiresCandidateRole()
    {
        var method = typeof(DashboardController)
            .GetMethod(nameof(DashboardController.GetCandidateStats));

        var authorize = Assert.Single(
            method!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(nameof(UserRole.Candidate), authorize.Roles);
    }

    [Fact]
    public async Task GetRecentApplicationsAsync_UsesCurrentUserAndReturnsRepositoryRows()
    {
        var expected = new List<RecentApplicationDto> { new() { ApplicationId = 5 } };
        var repository = new FakeCandidateDashboardRepository(new CandidateDashboardStatsDto(), expected);
        var service = new CandidateDashboardService(repository);

        var result = await service.GetRecentApplicationsAsync(42);

        Assert.Same(expected, result);
        Assert.Equal(42, repository.RequestedUserId);
    }

    [Fact]
    public void RecentApplicationsEndpoint_RequiresCandidateRole()
    {
        var method = typeof(DashboardController).GetMethod(nameof(DashboardController.GetRecentApplications));
        var authorize = Assert.Single(method!.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        Assert.Equal(nameof(UserRole.Candidate), authorize.Roles);
    }

    [Fact]
    public async Task GetRecommendedJobsAsync_UsesCurrentUserAndReturnsRepositoryRows()
    {
        var expected = new List<RecommendedJobDto> { new() { JobId = 9 } };
        var repository = new FakeCandidateDashboardRepository(new CandidateDashboardStatsDto(), jobs: expected);
        var service = new CandidateDashboardService(repository);

        var result = await service.GetRecommendedJobsAsync(42);

        Assert.Same(expected, result);
        Assert.Equal(42, repository.RequestedUserId);
    }

    [Fact]
    public void RecommendedJobsEndpoint_RequiresCandidateRole()
    {
        var method = typeof(DashboardController).GetMethod(nameof(DashboardController.GetRecommendedJobs));
        var authorize = Assert.Single(method!.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        Assert.Equal(nameof(UserRole.Candidate), authorize.Roles);
    }

    private sealed class FakeCandidateDashboardRepository(
        CandidateDashboardStatsDto stats,
        IReadOnlyList<RecentApplicationDto>? applications = null,
        IReadOnlyList<RecommendedJobDto>? jobs = null) : ICandidateDashboardRepository
    {
        public int RequestedUserId { get; private set; }

        public Task<CandidateDashboardStatsDto> GetStatsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            RequestedUserId = userId;
            return Task.FromResult(stats);
        }

        public Task<IReadOnlyList<RecentApplicationDto>> GetRecentApplicationsAsync(int userId, CancellationToken cancellationToken = default)
        {
            RequestedUserId = userId;
            return Task.FromResult(applications ?? (IReadOnlyList<RecentApplicationDto>)[]);
        }

        public Task<IReadOnlyList<RecommendedJobDto>> GetRecommendedJobsAsync(int userId, CancellationToken cancellationToken = default)
        {
            RequestedUserId = userId;
            return Task.FromResult(jobs ?? (IReadOnlyList<RecommendedJobDto>)[]);
        }
    }

    private sealed class FakeCandidateDashboardService(
        CandidateDashboardStatsDto stats) : ICandidateDashboardService
    {
        public int RequestedUserId { get; private set; }

        public Task<CandidateDashboardStatsDto> GetStatsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            RequestedUserId = userId;
            return Task.FromResult(stats);
        }

        public Task<IReadOnlyList<RecentApplicationDto>> GetRecentApplicationsAsync(int userId, CancellationToken cancellationToken = default)
        {
            RequestedUserId = userId;
            return Task.FromResult<IReadOnlyList<RecentApplicationDto>>([]);
        }

        public Task<IReadOnlyList<RecommendedJobDto>> GetRecommendedJobsAsync(int userId, CancellationToken cancellationToken = default)
        {
            RequestedUserId = userId;
            return Task.FromResult<IReadOnlyList<RecommendedJobDto>>([]);
        }
    }

    private sealed class FakeRecruiterDashboardService : IRecruiterDashboardService
    {
        public Task<RecruiterDashboardStatsDto?> GetStatsAsync(int userId, CancellationToken cancellationToken = default) => Task.FromResult<RecruiterDashboardStatsDto?>(new());
        public Task<IReadOnlyList<RecruiterRecentApplicantDto>> GetRecentApplicantsAsync(int userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<RecruiterRecentApplicantDto>>([]);
        public Task<IReadOnlyList<RecruiterUpcomingInterviewDto>> GetUpcomingInterviewsAsync(int userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<RecruiterUpcomingInterviewDto>>([]);
        public Task<RecruiterApplicationsOverviewDto> GetApplicationsOverviewAsync(int userId, CancellationToken cancellationToken = default) => Task.FromResult(new RecruiterApplicationsOverviewDto());
    }

    private sealed class FakeHiringManagerDashboardService : IHiringManagerDashboardService
    {
        public Task<HiringManagerDashboardStatsDto?> GetStatsAsync(int userId, CancellationToken token = default) =>
            Task.FromResult<HiringManagerDashboardStatsDto?>(new());
    }
}
