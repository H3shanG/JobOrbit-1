using JobOrbit.Application.Authorization;
using JobOrbit.Application.DTOs.AdminOrganizations;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController,Route("api/admin/organizations"),Authorize(Roles=nameof(UserRole.Administrator)),Authorize(Policy=PermissionConstants.AdminOrganizationsManage)]
public sealed class AdminOrganizationsController(IAdminOrganizationService service):ControllerBase
{
    bool Id(out int id)=>int.TryParse(User.FindFirst("UserId")?.Value,out id);
    [HttpGet]public async Task<IActionResult>List([FromQuery]AdminOrganizationQuery q,CancellationToken t){var result=await service.ListAsync(q,t);return result.Valid?Ok(result.Result):BadRequest(new ProblemDetails{Status=400,Title="Invalid organization status filter"});}
    [HttpGet("lookup")]public async Task<IActionResult>Lookup([FromQuery]bool includeInactive=false,CancellationToken t=default)=>Ok(await service.LookupAsync(includeInactive,t));
    [HttpGet("{organizationId:int}")]public async Task<IActionResult>Details(int organizationId,CancellationToken t)=>await service.DetailsAsync(organizationId,t)is{}x?Ok(x):NotFound(new ProblemDetails{Status=404,Title="Organization not found"});
    [HttpPost]public async Task<IActionResult>Create(CreateOrganizationRequest r,CancellationToken t){if(!Id(out var id))return Unauthorized();var result=await service.CreateAsync(id,r,t);return result.Outcome==AdminOrganizationOutcome.Success?CreatedAtAction(nameof(Details),new{organizationId=result.Organization!.OrganizationId},result.Organization):ConflictResult(result);}
    [HttpPut("{organizationId:int}")]public async Task<IActionResult>Update(int organizationId,UpdateOrganizationRequest r,CancellationToken t){if(!Id(out var id))return Unauthorized();var result=await service.UpdateAsync(id,organizationId,r,t);return result.Outcome==AdminOrganizationOutcome.Success?Ok(result.Organization):ConflictResult(result);}
    [HttpPatch("{organizationId:int}/status")]public async Task<IActionResult>Status(int organizationId,UpdateOrganizationStatusRequest r,CancellationToken t){if(!Id(out var id))return Unauthorized();var result=await service.StatusAsync(id,organizationId,r,t);return result.Outcome==AdminOrganizationOutcome.Success?Ok(result.Organization):ConflictResult(result);}
    IActionResult ConflictResult(AdminOrganizationResult x)=>x.Outcome switch{AdminOrganizationOutcome.NotFound=>NotFound(new ProblemDetails{Status=404,Title="Organization not found"}),AdminOrganizationOutcome.DuplicateCode=>Conflict(new ProblemDetails{Status=409,Title="Organization code already exists"}),_=>Conflict(new ProblemDetails{Status=409,Title="Organization name already exists"})};
}
