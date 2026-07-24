using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.Matching;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController]
public sealed class JobMatchingController(IJobMatchingService service):ControllerBase
{
 bool UserId(out int id)=>int.TryParse(User.FindFirst("UserId")?.Value,out id);
 [HttpGet("api/candidate/jobs/recommended"),Authorize(Roles=nameof(UserRole.Candidate))]
 public async Task<ActionResult<IReadOnlyList<JobRecommendationDto>>>Recommended([FromQuery]CandidateRecommendationFilter filter,CancellationToken token){if(!UserId(out var id))return Unauthorized();try{return Ok(await service.GetRecommendedJobsAsync(id,filter,token));}catch(ArgumentException e){return BadRequest(new ProblemDetails{Status=400,Title="Invalid recommendation filter",Detail=e.Message});}}
 [HttpGet("api/candidate/jobs/{jobId:int}/match"),Authorize(Roles=nameof(UserRole.Candidate))]
 public async Task<ActionResult<JobMatchResultDto>>CandidateMatch(int jobId,CancellationToken token)=>UserId(out var id)?await service.CalculateCandidateJobMatchAsync(id,jobId,token)is{}x?Ok(x):NotFound():Unauthorized();
 [HttpGet("api/recruiter/jobs/{jobId:int}/ranked-applicants"),Authorize(Roles=nameof(UserRole.Recruiter))]
 public async Task<ActionResult<PagedResultDto<RankedCandidateDto>>>Ranked(int jobId,[FromQuery]CandidateRankingFilter filter,CancellationToken token){if(!UserId(out var id))return Unauthorized();try{return await service.GetRankedApplicantsAsync(id,jobId,filter,token)is{}x?Ok(x):NotFound();}catch(ArgumentException e){return BadRequest(new ProblemDetails{Status=400,Title="Invalid ranking filter",Detail=e.Message});}}
 [HttpGet("api/recruiter/applications/{applicationId:int}/match"),Authorize(Roles=nameof(UserRole.Recruiter))]
 public async Task<ActionResult<JobMatchResultDto>>RecruiterMatch(int applicationId,CancellationToken token)=>UserId(out var id)?await service.GetRecruiterApplicationMatchAsync(id,applicationId,token)is{}x?Ok(x):NotFound():Unauthorized();
 [HttpGet("api/manager/applications/{applicationId:int}/match"),Authorize(Roles=nameof(UserRole.HiringManager))]
 public async Task<ActionResult<JobMatchResultDto>>ManagerMatch(int applicationId,CancellationToken token)=>UserId(out var id)?await service.GetManagerApplicationMatchAsync(id,applicationId,token)is{}x?Ok(x):NotFound():Unauthorized();
}
