using JobOrbit.Application.DTOs.RecruiterJobs;
using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace JobOrbit.API.Controllers;
[ApiController,Route("api/recruiter"),Authorize(Roles=nameof(UserRole.Recruiter))]
public sealed class RecruiterJobsController(IRecruiterJobService service):ControllerBase
{
 private bool UserId(out int id)=>int.TryParse(User.FindFirst("UserId")?.Value,out id);
 [HttpPost("jobs")]public async Task<ActionResult<RecruiterJobResponse>>Create(CreateRecruiterJobRequest request,CancellationToken token){if(!UserId(out var id))return Unauthorized();var result=await service.CreateAsync(id,request,token);return result.Outcome switch{CreateRecruiterJobOutcome.Created=>CreatedAtAction(nameof(Get),new{jobId=result.Job!.JobId},result.Job),CreateRecruiterJobOutcome.InvalidDepartment=>NotFound(new ProblemDetails{Status=404,Title="Department not found"}),CreateRecruiterJobOutcome.InvalidSkills=>NotFound(new ProblemDetails{Status=404,Title="One or more skills were not found"}),_=>BadRequest(new ProblemDetails{Status=400,Title="Recruiter profile required"})};}
 [HttpGet("jobs")]public async Task<ActionResult<PagedResultDto<RecruiterJobListItemDto>>>List([FromQuery]RecruiterJobQuery query,CancellationToken token){if(!UserId(out var id))return Unauthorized();if(!string.IsNullOrWhiteSpace(query.Status)&&!Enum.TryParse<JobStatus>(query.Status,true,out _))return BadRequest(new ProblemDetails{Status=400,Title="Invalid job status"});return Ok(await service.ListAsync(id,query,token));}
 [HttpGet("jobs/{jobId:int}")]public async Task<ActionResult<RecruiterJobDetailsDto>>Get(int jobId,CancellationToken token)=>UserId(out var id)?(await service.DetailsAsync(id,jobId,token)is{}job?Ok(job):NotFound()):Unauthorized();
 [HttpPut("jobs/{jobId:int}")]public async Task<IActionResult>Update(int jobId,UpdateRecruiterJobRequest request,CancellationToken token)=>UserId(out var id)?Mutation(await service.UpdateAsync(id,jobId,request,token)):Unauthorized();
 [HttpPatch("jobs/{jobId:int}/publish")]public async Task<IActionResult>Publish(int jobId,CancellationToken token)=>UserId(out var id)?Mutation(await service.PublishAsync(id,jobId,token)):Unauthorized();
 [HttpPatch("jobs/{jobId:int}/close")]public async Task<IActionResult>Close(int jobId,CancellationToken token)=>UserId(out var id)?Mutation(await service.CloseAsync(id,jobId,token)):Unauthorized();
 [HttpDelete("jobs/{jobId:int}")]public async Task<IActionResult>Delete(int jobId,CancellationToken token)=>UserId(out var id)?Mutation(await service.DeleteAsync(id,jobId,token)):Unauthorized();
 [HttpGet("departments")]public async Task<ActionResult<IReadOnlyList<RecruiterReferenceDto>>>Departments(CancellationToken token)=>UserId(out var id)?Ok(await service.DepartmentsAsync(id,token)):Unauthorized();
 [HttpGet("skills")]public async Task<ActionResult<IReadOnlyList<RecruiterReferenceDto>>>Skills(CancellationToken token)=>Ok(await service.SkillsAsync(token));
 private IActionResult Mutation(RecruiterJobMutationOutcome outcome)=>outcome switch{RecruiterJobMutationOutcome.Success=>NoContent(),RecruiterJobMutationOutcome.NotFound=>NotFound(),RecruiterJobMutationOutcome.InvalidDepartment=>NotFound(new ProblemDetails{Status=404,Title="Department not found"}),RecruiterJobMutationOutcome.InvalidSkills=>NotFound(new ProblemDetails{Status=404,Title="One or more skills were not found"}),RecruiterJobMutationOutcome.InvalidTransition=>Conflict(new ProblemDetails{Status=409,Title="The requested job status transition is not allowed"}),RecruiterJobMutationOutcome.HasApplications=>Conflict(new ProblemDetails{Status=409,Title="A job with applications cannot be deleted"}),_=>BadRequest()};
}
