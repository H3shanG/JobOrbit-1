using JobOrbit.Application.DTOs.HiringManagerEvaluations;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController, Authorize(Roles = nameof(UserRole.HiringManager))]
public sealed class HiringManagerEvaluationsController(IHiringManagerEvaluationService service) : ControllerBase
{
    private bool UserId(out int id) => int.TryParse(User.FindFirst("UserId")?.Value, out id);
    [HttpPost("api/manager/applications/{applicationId:int}/evaluations")]
    public async Task<ActionResult<CandidateEvaluationDto>> Create(int applicationId, CreateCandidateEvaluationRequest request, CancellationToken token)
    { if (!UserId(out var id)) return Unauthorized(); var result = await service.CreateAsync(id, applicationId, request, token); return result.Outcome switch { EvaluationMutationOutcome.Success => CreatedAtAction(nameof(List), new { applicationId }, result.Evaluation), EvaluationMutationOutcome.NotFound => NotFound(), EvaluationMutationOutcome.Duplicate => Conflict(new ProblemDetails { Status = 409, Title = "An evaluation already exists for this application" }), EvaluationMutationOutcome.InvalidState => Conflict(new ProblemDetails { Status = 409, Title = "The application is not ready for evaluation" }), _ => BadRequest(new ProblemDetails { Status = 400, Title = "Recommendation must be Proceed, Hold, or Reject" }) }; }
    [HttpGet("api/manager/applications/{applicationId:int}/evaluations")]
    public async Task<ActionResult<IReadOnlyList<CandidateEvaluationDto>>> List(int applicationId, CancellationToken token) { if (!UserId(out var id)) return Unauthorized(); var result = await service.ListAsync(id, applicationId, token); return result is null ? NotFound() : Ok(result); }
    [HttpPut("api/manager/evaluations/{evaluationId:int}")]
    public async Task<ActionResult<CandidateEvaluationDto>> Update(int evaluationId, UpdateCandidateEvaluationRequest request, CancellationToken token) { if (!UserId(out var id)) return Unauthorized(); var result = await service.UpdateAsync(id, evaluationId, request, token); return result.Outcome == EvaluationMutationOutcome.Success ? Ok(result.Evaluation) : result.Outcome == EvaluationMutationOutcome.NotFound ? NotFound() : BadRequest(new ProblemDetails { Status = 400, Title = "Recommendation must be Proceed, Hold, or Reject" }); }
    [HttpGet("api/dashboard/hiring-manager/evaluation-summary")]
    public async Task<ActionResult<HiringManagerEvaluationSummaryDto>> Summary(CancellationToken token) => UserId(out var id) ? Ok(await service.SummaryAsync(id, token)) : Unauthorized();
}
