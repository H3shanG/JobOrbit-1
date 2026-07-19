using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.Applications;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize(Roles = nameof(UserRole.Candidate))]
public sealed class JobsController(
    IJobService jobService,
    IJobApplicationService jobApplicationService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResultDto<JobListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResultDto<JobListItemDto>>> GetJobs(
        [FromQuery] JobListQueryDto query,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId))
        {
            return Unauthorized();
        }

        return Ok(await jobService.GetJobsAsync(userId, query, cancellationToken));
    }

    [HttpGet("{jobId:int}")]
    [ProducesResponseType<JobDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobDetailsDto>> GetJob(
        int jobId,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId))
        {
            return Unauthorized();
        }

        var job = await jobService.GetJobDetailsAsync(userId, jobId, cancellationToken);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost("{jobId:int}/applications")]
    [ProducesResponseType<JobApplicationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<JobApplicationResponse>> Apply(
        int jobId,
        [FromBody] CreateJobApplicationRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId))
        {
            return Unauthorized();
        }

        var result = await jobApplicationService.ApplyAsync(
            userId, jobId, request, cancellationToken);

        return result.Outcome switch
        {
            CreateApplicationOutcome.Created => CreatedAtAction(
                nameof(GetJob), new { jobId }, result.Application),
            CreateApplicationOutcome.JobUnavailable => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Job unavailable",
                Detail = "This job does not exist or is no longer accepting applications."
            }),
            CreateApplicationOutcome.CandidateProfileMissing => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Candidate profile required",
                Detail = "A candidate profile is required before applying."
            }),
            CreateApplicationOutcome.InvalidResume => BadRequest(new ProblemDetails { Status = 400, Title = "Invalid resume", Detail = "The selected resume is unavailable." }),
            CreateApplicationOutcome.ProfileIncomplete => Conflict(new ProblemDetails { Status = 409, Title = "Profile incomplete", Detail = "Complete the required candidate profile fields before applying." }),
            _ => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Already applied",
                Detail = "You have already applied for this job."
            })
        };
    }
}
