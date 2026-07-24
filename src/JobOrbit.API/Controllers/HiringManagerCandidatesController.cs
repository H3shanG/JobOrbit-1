using JobOrbit.Application.DTOs.HiringManagerCandidates;
using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController]
[Route("api/manager/candidates")]
[Authorize(Roles = nameof(UserRole.HiringManager))]
public sealed class HiringManagerCandidatesController(IHiringManagerCandidateService service) : ControllerBase
{
    private bool TryUserId(out int userId) => int.TryParse(User.FindFirst("UserId")?.Value, out userId);

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<HiringManagerCandidateListItemDto>>> List([FromQuery] HiringManagerCandidateQuery query, CancellationToken token)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        if (!string.IsNullOrWhiteSpace(query.Status) && !Enum.TryParse<ApplicationStatus>(query.Status, true, out _))
            return BadRequest(new ProblemDetails { Status = 400, Title = "Invalid application status" });
        return Ok(await service.ListAsync(userId, query, token));
    }

    [HttpGet("{applicationId:int}")]
    public async Task<ActionResult<HiringManagerCandidateDetailsDto>> Details(int applicationId, CancellationToken token)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        var result = await service.DetailsAsync(userId, applicationId, token);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{applicationId:int}/resume")]
    public async Task<IActionResult> DownloadResume(int applicationId, CancellationToken token)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        var result = await service.DownloadResumeAsync(userId, applicationId, token);
        return result is null ? NotFound() : File(result.Content, result.ContentType, result.OriginalFileName);
    }

    [HttpGet("~/api/dashboard/hiring-manager/candidates-to-review")]
    public async Task<ActionResult<IReadOnlyList<HiringManagerDashboardCandidateDto>>> Latest(CancellationToken token)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        return Ok(await service.LatestAsync(userId, token));
    }
}
