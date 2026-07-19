using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.RecruiterApplications;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace JobOrbit.API.Controllers;
[ApiController,Route("api/recruiter/applications"),Authorize(Roles=nameof(UserRole.Recruiter))]
public sealed class RecruiterApplicationsController(IRecruiterApplicationService service):ControllerBase
{
 private bool UserId(out int id)=>int.TryParse(User.FindFirst("UserId")?.Value,out id);
 [HttpGet]public async Task<ActionResult<PagedResultDto<RecruiterApplicationListItemDto>>>List([FromQuery]RecruiterApplicationQuery query,CancellationToken token){if(!UserId(out var id))return Unauthorized();if(!string.IsNullOrWhiteSpace(query.Status)&&!Enum.TryParse<ApplicationStatus>(query.Status,true,out _))return BadRequest(new ProblemDetails{Status=400,Title="Invalid application status"});return Ok(await service.ListAsync(id,query,token));}
 [HttpGet("{applicationId:int}")]public async Task<ActionResult<RecruiterApplicationDetailsDto>>Details(int applicationId,CancellationToken token)=>UserId(out var id)?(await service.DetailsAsync(id,applicationId,token)is{}result?Ok(result):NotFound()):Unauthorized();
 [HttpPatch("{applicationId:int}/status")]public async Task<IActionResult>UpdateStatus(int applicationId,UpdateApplicationStatusRequest request,CancellationToken token){if(!UserId(out var id))return Unauthorized();return(await service.UpdateStatusAsync(id,applicationId,request.Status,token))switch{RecruiterApplicationMutationOutcome.Success=>NoContent(),RecruiterApplicationMutationOutcome.NotFound=>NotFound(),RecruiterApplicationMutationOutcome.InvalidStatus=>BadRequest(new ProblemDetails{Status=400,Title="Invalid application status"}),RecruiterApplicationMutationOutcome.InvalidTransition=>Conflict(new ProblemDetails{Status=409,Title="The requested status transition is not allowed"}),_=>BadRequest()};}
 [HttpGet("{applicationId:int}/resume")]public async Task<IActionResult>DownloadResume(int applicationId,CancellationToken token){if(!UserId(out var id))return Unauthorized();var result=await service.DownloadResumeAsync(id,applicationId,token);return result is null?NotFound():File(result.Content,result.ContentType,result.OriginalFileName);}
}
