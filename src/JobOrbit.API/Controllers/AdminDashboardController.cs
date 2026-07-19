using JobOrbit.Application.DTOs.AdminDashboard;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController, Route("api/dashboard/admin"), Authorize(Roles = nameof(UserRole.Administrator))]
public sealed class AdminDashboardController(IAdminDashboardService service) : ControllerBase
{
    private bool HasUserId() => int.TryParse(User.FindFirst("UserId")?.Value, out _);
    [HttpGet("stats")] public async Task<ActionResult<AdminDashboardStatsDto>> Stats(CancellationToken token) => HasUserId() ? Ok(await service.StatsAsync(token)) : Unauthorized();
    [HttpGet("user-growth")]
    public async Task<IActionResult> Growth(DateTime? from, DateTime? to, CancellationToken token)
    {
        if (!HasUserId()) return Unauthorized();
        var end = (to ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
        var start = (from ?? end.AddMonths(-6)).Date;
        if (start > end || end - start > TimeSpan.FromDays(366 * 2)) return BadRequest(new ProblemDetails { Status = 400, Title = "Invalid date range" });
        return Ok(await service.UserGrowthAsync(DateTime.SpecifyKind(start, DateTimeKind.Utc), DateTime.SpecifyKind(end, DateTimeKind.Utc), token));
    }
    [HttpGet("application-overview")] public async Task<IActionResult> Applications(CancellationToken token) => HasUserId() ? Ok(await service.ApplicationOverviewAsync(token)) : Unauthorized();
    [HttpGet("recent-activity")] public async Task<IActionResult> Activity(int limit = 10, CancellationToken token = default) => HasUserId() ? Ok(await service.RecentActivityAsync(limit, token)) : Unauthorized();
    [HttpGet("system-health")] public async Task<IActionResult> Health(CancellationToken token) => HasUserId() ? Ok(await service.SystemHealthAsync(token)) : Unauthorized();
}
