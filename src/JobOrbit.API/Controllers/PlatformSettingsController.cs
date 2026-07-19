using JobOrbit.Application.DTOs.AdminSystemSettings;
using JobOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController,Route("api/platform-settings")]
public sealed class PlatformSettingsController(ISystemSettingsProvider provider):ControllerBase
{
 [AllowAnonymous,HttpGet("public")]
 public async Task<ActionResult<PublicPlatformSettingsDto>>Public(CancellationToken token)
 {var x=await provider.GetAsync(token);return Ok(new PublicPlatformSettingsDto(x.General.PlatformName,x.Recruitment.AllowCandidateSelfRegistration,x.Recruitment.DefaultJobClosingDays,x.Maintenance.MaintenanceModeEnabled,x.Maintenance.MaintenanceModeEnabled?x.Maintenance.MaintenanceMessage:null));}
}
