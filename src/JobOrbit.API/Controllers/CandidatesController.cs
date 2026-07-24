using JobOrbit.Application.DTOs.Candidates;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController]
[Route("api/candidates/me")]
[Authorize(Roles = nameof(UserRole.Candidate))]
public sealed class CandidatesController(ICandidateProfileService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CandidateProfileDto>> Get(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId)) return Unauthorized();
        var profile = await service.GetAsync(userId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<CandidateProfileDto>> Update(
        [FromBody] UpdateCandidateProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId)) return Unauthorized();
        var profile = await service.UpdateAsync(userId, request, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }
}
