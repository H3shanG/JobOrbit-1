using JobOrbit.Application.DTOs.Dashboard;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(
    ICandidateDashboardService candidateDashboardService,
    IRecruiterDashboardService recruiterDashboardService,
    IHiringManagerDashboardService hiringManagerDashboardService) : ControllerBase
{
    [Authorize(Roles = nameof(UserRole.HiringManager))]
    [HttpGet("hiring-manager/stats")]
    public async Task<ActionResult<HiringManagerDashboardStatsDto>> GetHiringManagerStats(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId)) return Unauthorized();
        var stats = await hiringManagerDashboardService.GetStatsAsync(userId, cancellationToken);
        return stats is null
            ? NotFound(new ProblemDetails { Status = 404, Title = "Hiring Manager account not found" })
            : Ok(stats);
    }

    [Authorize(Roles = nameof(UserRole.Recruiter))]
    [HttpGet("recruiter/stats")]
    public async Task<ActionResult<RecruiterDashboardStatsDto>> GetRecruiterStats(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId)) return Unauthorized();
        var stats = await recruiterDashboardService.GetStatsAsync(userId, cancellationToken);
        return stats is null ? NotFound(new ProblemDetails { Status=404, Title="Recruiter profile not found" }) : Ok(stats);
    }

    [Authorize(Roles = nameof(UserRole.Recruiter))]
    [HttpGet("recruiter/recent-applicants")]
    public async Task<ActionResult<IReadOnlyList<RecruiterRecentApplicantDto>>> GetRecruiterRecentApplicants(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId)) return Unauthorized();
        return Ok(await recruiterDashboardService.GetRecentApplicantsAsync(userId, cancellationToken));
    }

    [Authorize(Roles = nameof(UserRole.Recruiter))]
    [HttpGet("recruiter/upcoming-interviews")]
    public async Task<ActionResult<IReadOnlyList<RecruiterUpcomingInterviewDto>>> GetRecruiterUpcomingInterviews(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId)) return Unauthorized();
        return Ok(await recruiterDashboardService.GetUpcomingInterviewsAsync(userId, cancellationToken));
    }

    [Authorize(Roles = nameof(UserRole.Recruiter))]
    [HttpGet("recruiter/applications-overview")]
    public async Task<ActionResult<RecruiterApplicationsOverviewDto>> GetRecruiterApplicationsOverview(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId)) return Unauthorized();
        return Ok(await recruiterDashboardService.GetApplicationsOverviewAsync(userId, cancellationToken));
    }

    [Authorize(Roles = nameof(UserRole.Candidate))]
    [HttpGet("candidate/stats")]
    [ProducesResponseType<CandidateDashboardStatsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CandidateDashboardStatsDto>> GetCandidateStats(
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var stats = await candidateDashboardService.GetStatsAsync(
            userId,
            cancellationToken);

        return Ok(stats);
    }

    [Authorize(Roles = nameof(UserRole.Candidate))]
    [HttpGet("candidate/recent-applications")]
    [ProducesResponseType<IReadOnlyList<RecentApplicationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<RecentApplicationDto>>> GetRecentApplications(
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId))
        {
            return Unauthorized();
        }

        return Ok(await candidateDashboardService.GetRecentApplicationsAsync(
            userId,
            cancellationToken));
    }

    [Authorize(Roles = nameof(UserRole.Candidate))]
    [HttpGet("candidate/recommended-jobs")]
    [ProducesResponseType<IReadOnlyList<RecommendedJobDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<RecommendedJobDto>>> GetRecommendedJobs(
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId))
        {
            return Unauthorized();
        }

        return Ok(await candidateDashboardService.GetRecommendedJobsAsync(
            userId,
            cancellationToken));
    }
}
