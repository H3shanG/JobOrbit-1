using JobOrbit.Application.DTOs.Candidates;
using JobOrbit.Application.Interfaces;
using JobOrbit.Application.Services;
using JobOrbit.API.Models;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController]
[Route("api/candidates/me/resumes")]
[Authorize(Roles = nameof(UserRole.Candidate))]
public sealed class CandidateResumesController(ICandidateResumeService service) : ControllerBase
{
    private bool TryUserId(out int userId) => int.TryParse(User.FindFirst("UserId")?.Value, out userId);
    [HttpGet] public async Task<ActionResult<IReadOnlyList<CandidateResumeDto>>> List(CancellationToken token) => TryUserId(out var id) ? Ok(await service.ListAsync(id, token)) : Unauthorized();
    [HttpPost, Consumes("multipart/form-data"), RequestSizeLimit(21 * 1024 * 1024)]
    public async Task<ActionResult<CandidateResumeDto>> Upload([FromForm] UploadResumeRequest request, CancellationToken token)
    {
        if (!TryUserId(out var id)) return Unauthorized();
        var file = request.File;
        try { await using var stream = file.OpenReadStream(); var result = await service.UploadAsync(id, stream, file.FileName, file.ContentType, file.Length, request.DisplayName, token); return result is null ? BadRequest() : StatusCode(201, result); }
        catch (ArgumentException ex) { return BadRequest(new ProblemDetails { Status = 400, Title = "Invalid resume", Detail = ex.Message }); }
    }
    [HttpGet("{resumeId:int}")] public async Task<IActionResult> Download(int resumeId, CancellationToken token)
    {
        if (!TryUserId(out var id)) return Unauthorized(); var result = await service.DownloadAsync(id, resumeId, token);
        return result is null ? NotFound() : File(result.Content, result.ContentType, result.OriginalFileName);
    }
    [HttpDelete("{resumeId:int}")] public async Task<IActionResult> Delete(int resumeId, CancellationToken token)
    {
        if (!TryUserId(out var id)) return Unauthorized(); var outcome = await service.DeleteAsync(id, resumeId, token);
        return outcome switch { DeleteResumeOutcome.Deleted => NoContent(), DeleteResumeOutcome.Referenced => Conflict(new ProblemDetails { Status = 409, Title = "Resume is in use", Detail = "This resume is referenced by a submitted application and cannot be deleted." }), _ => NotFound() };
    }
    [HttpPatch("{resumeId:int}/default")] public async Task<IActionResult> SetDefault(int resumeId, CancellationToken token)
    {
        if (!TryUserId(out var id)) return Unauthorized(); return await service.SetDefaultAsync(id, resumeId, token) ? NoContent() : NotFound();
    }
}
