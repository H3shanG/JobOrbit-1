using JobOrbit.Application.DTOs.Candidates;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController, Route("api/candidates/me"), Authorize(Roles=nameof(UserRole.Candidate))]
public sealed class CandidateSettingsController(ICandidateSettingsService service) : ControllerBase
{
    private bool TryUserId(out int id) => int.TryParse(User.FindFirst("UserId")?.Value, out id);
    [HttpGet("settings")]
    public async Task<ActionResult<CandidateSettingsDto>> Get(CancellationToken token) => TryUserId(out var id) ? (await service.GetAsync(id,token) is { } value ? Ok(value) : NotFound()) : Unauthorized();
    [HttpPut("settings")]
    public async Task<ActionResult<CandidateSettingsDto>> Update(UpdateCandidateSettingsRequest request, CancellationToken token) => TryUserId(out var id) ? (await service.UpdateAsync(id,request,token) is { } value ? Ok(value) : NotFound()) : Unauthorized();
    [HttpPut("password")]
    public async Task<IActionResult> Password(ChangePasswordRequest request, CancellationToken token)
    {
        if (!TryUserId(out var id)) return Unauthorized();
        return await service.ChangePasswordAsync(id,request,token) switch
        {
            ChangePasswordOutcome.Changed => NoContent(),
            ChangePasswordOutcome.IncorrectCurrentPassword => Unauthorized(new ProblemDetails { Status=401, Title="Current password is incorrect" }),
            _ => NotFound()
        };
    }
}
