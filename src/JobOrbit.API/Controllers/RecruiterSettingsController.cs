using JobOrbit.Application.DTOs.RecruiterSettings;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController]
[Route("api/recruiters/me")]
[Authorize(Roles = nameof(UserRole.Recruiter))]
public sealed class RecruiterSettingsController(IRecruiterSettingsService service) : ControllerBase
{
    private bool UserId(out int id) => int.TryParse(User.FindFirst("UserId")?.Value, out id);

    [HttpGet("settings")]
    public async Task<ActionResult<RecruiterSettingsDto>> Get(CancellationToken token) =>
        UserId(out var id) ? (await service.GetAsync(id, token) is { } value ? Ok(value) : NotFound()) : Unauthorized();

    [HttpPut("settings")]
    public async Task<ActionResult<RecruiterSettingsDto>> Update(UpdateRecruiterSettingsRequest request, CancellationToken token) =>
        UserId(out var id) ? (await service.UpdateAsync(id, request, token) is { } value ? Ok(value) : NotFound()) : Unauthorized();

    [HttpPut("password")]
    public async Task<IActionResult> Password(ChangeRecruiterPasswordRequest request, CancellationToken token)
    {
        if (!UserId(out var id)) return Unauthorized();
        return await service.ChangePasswordAsync(id, request, token) switch
        {
            RecruiterPasswordOutcome.Changed => NoContent(),
            RecruiterPasswordOutcome.IncorrectCurrentPassword => Unauthorized(new ProblemDetails { Status = 401, Title = "Current password is incorrect" }),
            _ => NotFound()
        };
    }
}
