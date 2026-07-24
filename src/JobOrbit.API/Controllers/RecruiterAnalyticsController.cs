using JobOrbit.Application.DTOs.RecruiterAnalytics;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController]
[Route("api/recruiter/analytics")]
[Authorize(Roles = nameof(UserRole.Recruiter))]
public sealed class RecruiterAnalyticsController(IRecruiterAnalyticsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<RecruiterAnalyticsDto>> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken token)
    {
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId)) return Unauthorized();
        var now = DateTime.UtcNow;
        var rangeFrom = from ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);
        var rangeTo = to ?? now;
        if (rangeFrom.Kind != DateTimeKind.Utc) rangeFrom = rangeFrom.ToUniversalTime();
        if (rangeTo.Kind != DateTimeKind.Utc) rangeTo = rangeTo.ToUniversalTime();
        if (to.HasValue && rangeTo.TimeOfDay == TimeSpan.Zero) rangeTo = rangeTo.Date.AddDays(1).AddTicks(-1);
        if (rangeFrom > rangeTo)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Invalid date range", Detail = "The from date must be before or equal to the to date." });
        if (rangeTo > rangeFrom.AddYears(5))
            return BadRequest(new ProblemDetails { Status = 400, Title = "Invalid date range", Detail = "Analytics date ranges cannot exceed five years." });
        return Ok(await service.GetAsync(userId, rangeFrom, rangeTo, token));
    }
}
