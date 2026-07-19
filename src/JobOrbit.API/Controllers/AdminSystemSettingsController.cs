using JobOrbit.Application.Authorization;
using JobOrbit.Application.DTOs.AdminSystemSettings;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController,Route("api/admin/system-settings"),Authorize(Roles=nameof(UserRole.Administrator)),Authorize(Policy=PermissionConstants.AdminSettingsManage)]
public sealed class AdminSystemSettingsController(IAdminSystemSettingsService service):ControllerBase
{
 int Actor()=>int.TryParse(User.FindFirst("UserId")?.Value,out var id)?id:0;
 IActionResult Result(SystemSettingsMutationResult x)=>x.Success?Ok(x.Data):x.Conflict?Conflict(new ProblemDetails{Status=409,Title="Unsafe settings combination",Detail=x.Error}):BadRequest(new ProblemDetails{Status=400,Title="Invalid settings",Detail=x.Error});
 [HttpGet]public async Task<ActionResult<SystemSettingsDto>>Get(CancellationToken t)=>Ok(await service.GetAsync(t));
 [HttpPut("general")]public async Task<IActionResult>General(UpdateGeneralSettingsRequest r,CancellationToken t)=>Result(await service.UpdateGeneralAsync(Actor(),r,t));
 [HttpPut("recruitment")]public async Task<IActionResult>Recruitment(UpdateRecruitmentSettingsRequest r,CancellationToken t)=>Result(await service.UpdateRecruitmentAsync(Actor(),r,t));
 [HttpPut("uploads")]public async Task<IActionResult>Uploads(UpdateUploadSettingsRequest r,CancellationToken t)=>Result(await service.UpdateUploadsAsync(Actor(),r,t));
 [HttpPut("security")]public async Task<IActionResult>Security(UpdateSecuritySettingsRequest r,CancellationToken t)=>Result(await service.UpdateSecurityAsync(Actor(),r,t));
 [HttpPut("notifications")]public async Task<IActionResult>Notifications(UpdateNotificationSettingsRequest r,CancellationToken t)=>Result(await service.UpdateNotificationsAsync(Actor(),r,t));
 [HttpPut("maintenance")]public async Task<IActionResult>Maintenance(UpdateMaintenanceSettingsRequest r,CancellationToken t)=>Result(await service.UpdateMaintenanceAsync(Actor(),r,t));
 [HttpPost("reset/{section}")]public async Task<IActionResult>Reset(string section,CancellationToken t)=>Result(await service.ResetAsync(Actor(),section,t));
}
