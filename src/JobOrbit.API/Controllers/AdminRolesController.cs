using JobOrbit.Application.Authorization;
using JobOrbit.Application.DTOs.AdminRoles;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController,Route("api/admin"),Authorize(Roles=nameof(UserRole.Administrator))]
public sealed class AdminRolesController(IAdminRoleService service):ControllerBase
{
    private bool Id(out int id)=>int.TryParse(User.FindFirst("UserId")?.Value,out id);

    [HttpGet("roles"),Authorize(Policy=PermissionConstants.AdminRolesView)]
    public async Task<IActionResult>List(CancellationToken token)=>Ok(await service.ListAsync(token));

    [HttpGet("roles/{roleName}"),Authorize(Policy=PermissionConstants.AdminRolesView)]
    public async Task<IActionResult>Details(string roleName,CancellationToken token)=>await service.DetailsAsync(roleName,token)is{}role?Ok(role):NotFound(new ProblemDetails{Status=404,Title="Role not found"});

    [HttpGet("permissions"),Authorize(Policy=PermissionConstants.AdminRolesView)]
    public async Task<IActionResult>Permissions([FromQuery]string?category,[FromQuery]string?search,CancellationToken token)=>Ok(await service.PermissionsAsync(category,search,token));

    [HttpPut("roles/{roleName}/permissions"),Authorize(Policy=PermissionConstants.AdminRolesManage)]
    public async Task<IActionResult>Update(string roleName,UpdateRolePermissionsRequest request,CancellationToken token)
    {
        if(!Id(out var id))return Unauthorized();
        return Result(await service.UpdateAsync(id,roleName,request,token));
    }

    [HttpPost("roles/{roleName}/permissions/reset"),Authorize(Policy=PermissionConstants.AdminRolesManage)]
    public async Task<IActionResult>Reset(string roleName,CancellationToken token)
    {
        if(!Id(out var id))return Unauthorized();
        return Result(await service.ResetAsync(id,roleName,token));
    }

    private IActionResult Result(AdminRoleUpdateResult result)=>result.Outcome switch
    {
        AdminRoleUpdateOutcome.Success=>Ok(result.Role),
        AdminRoleUpdateOutcome.UnknownRole=>NotFound(new ProblemDetails{Status=404,Title="Role not found"}),
        AdminRoleUpdateOutcome.UnknownPermission=>BadRequest(new ProblemDetails{Status=400,Title="Unknown permission code",Detail=result.InvalidCode}),
        AdminRoleUpdateOutcome.DuplicatePermission=>BadRequest(new ProblemDetails{Status=400,Title="Duplicate permission codes are not allowed"}),
        AdminRoleUpdateOutcome.IncompatiblePermission=>BadRequest(new ProblemDetails{Status=400,Title="Permission is incompatible with this role",Detail=result.InvalidCode}),
        _=>Conflict(new ProblemDetails{Status=409,Title="Mandatory permissions cannot be removed"})
    };
}
