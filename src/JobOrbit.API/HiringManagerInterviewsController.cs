using JobOrbit.Application.DTOs.HiringManagerInterviews;using JobOrbit.Application.DTOs.Jobs;using JobOrbit.Application.Interfaces;using JobOrbit.Domain.Enums;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace JobOrbit.API.Controllers;
[ApiController,Route("api/manager/interviews"),Authorize(Roles=nameof(UserRole.HiringManager))]
public sealed class HiringManagerInterviewsController(IHiringManagerInterviewService service):ControllerBase
{
 bool UserId(out int id)=>int.TryParse(User.FindFirst("UserId")?.Value,out id);
 [HttpGet]public async Task<ActionResult<PagedResultDto<HiringManagerInterviewListItemDto>>>List([FromQuery]HiringManagerInterviewQuery query,CancellationToken token){if(!UserId(out var id))return Unauthorized();if(!string.IsNullOrWhiteSpace(query.Status)&&!Enum.TryParse<InterviewStatus>(query.Status,true,out _))return BadRequest(new ProblemDetails{Status=400,Title="Invalid interview status"});if(query.From.HasValue&&query.To.HasValue&&query.From>query.To)return BadRequest(new ProblemDetails{Status=400,Title="The from date must be before the to date"});return Ok(await service.ListAsync(id,query,token));}
 [HttpGet("{interviewId:int}")]public async Task<ActionResult<HiringManagerInterviewDetailsDto>>Details(int interviewId,CancellationToken token){if(!UserId(out var id))return Unauthorized();var result=await service.DetailsAsync(id,interviewId,token);return result is null?NotFound():Ok(result);}
}
