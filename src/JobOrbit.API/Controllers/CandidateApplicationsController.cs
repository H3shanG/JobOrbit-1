using JobOrbit.Application.DTOs.Applications;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController]
[Route("api/candidates/me/applications")]
[Authorize(Roles = nameof(UserRole.Candidate))]
public sealed class CandidateApplicationsController(
    ICandidateApplicationService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<CandidateApplicationsPageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CandidateApplicationsPageDto>> GetApplications(
        [FromQuery] CandidateApplicationQueryDto query,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId)) return Unauthorized();
        return Ok(await service.GetApplicationsAsync(userId, query, cancellationToken));
    }

    [HttpGet("{applicationId:int}")]
    [ProducesResponseType<CandidateApplicationDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CandidateApplicationDetailsDto>> GetApplication(
        int applicationId,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId)) return Unauthorized();
        var application = await service.GetApplicationAsync(userId, applicationId, cancellationToken);
        return application is null ? NotFound() : Ok(application);
    }
}
