using JobOrbit.Application.DTOs.HiringDecisions;
using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController,Route("api/manager/hiring-decisions"),Authorize(Roles=nameof(UserRole.HiringManager))]
public sealed class HiringDecisionsController(IHiringDecisionService service):ControllerBase
{
 bool Id(out int id)=>int.TryParse(User.FindFirst("UserId")?.Value,out id);
 [HttpGet] public async Task<ActionResult<PagedResultDto<HiringDecisionListItemDto>>>List([FromQuery]HiringDecisionQuery q,CancellationToken t){if(!Id(out var id))return Unauthorized();if(!string.IsNullOrWhiteSpace(q.Decision)&&!q.Decision.Equals("Pending",StringComparison.OrdinalIgnoreCase)&&!Enum.TryParse<ManagerHiringDecision>(q.Decision,true,out _))return BadRequest();return Ok(await service.ListAsync(id,q,t));}
 [HttpGet("{applicationId:int}")] public async Task<ActionResult<HiringDecisionDetailsDto>>Details(int applicationId,CancellationToken t)=>Id(out var id)?await service.DetailsAsync(id,applicationId,t)is{}x?Ok(x):NotFound():Unauthorized();
 [HttpPost("~/api/manager/applications/{applicationId:int}/hiring-decision")] public Task<IActionResult>Create(int applicationId,CreateHiringDecisionRequest r,CancellationToken t)=>Mutate(applicationId,r,true,t);
 [HttpPut("~/api/manager/applications/{applicationId:int}/hiring-decision")] public Task<IActionResult>Update(int applicationId,UpdateHiringDecisionRequest r,CancellationToken t)=>Mutate(applicationId,r,false,t);
 async Task<IActionResult>Mutate(int app,HiringDecisionRequest r,bool create,CancellationToken t){if(!Id(out var id))return Unauthorized();var x=create?await service.CreateAsync(id,app,(CreateHiringDecisionRequest)r,t):await service.UpdateAsync(id,app,(UpdateHiringDecisionRequest)r,t);return x.Outcome switch{HiringDecisionMutationOutcome.Success when create=>CreatedAtAction(nameof(Details),new{applicationId=app},x.Decision),HiringDecisionMutationOutcome.Success=>Ok(x.Decision),HiringDecisionMutationOutcome.NotFound=>NotFound(),HiringDecisionMutationOutcome.NoEvaluation=>Conflict(new ProblemDetails{Status=409,Title="A completed evaluation is required"}),HiringDecisionMutationOutcome.NoInterview=>Conflict(new ProblemDetails{Status=409,Title="A completed interview is required"}),HiringDecisionMutationOutcome.DuplicateFinal=>Conflict(new ProblemDetails{Status=409,Title="A final decision already exists"}),HiringDecisionMutationOutcome.InvalidTransition=>Conflict(new ProblemDetails{Status=409,Title="The decision transition is not allowed"}),_=>BadRequest(new ProblemDetails{Status=400,Title="Decision must be Hire, Reject, or Hold"})};}
 [HttpGet("~/api/dashboard/hiring-manager/hiring-funnel")] public async Task<ActionResult<HiringFunnelDto>>Funnel(CancellationToken t)=>Id(out var id)?Ok(await service.FunnelAsync(id,t)):Unauthorized();
}
