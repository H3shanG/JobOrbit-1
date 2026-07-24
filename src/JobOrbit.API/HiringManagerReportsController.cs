using JobOrbit.Application.DTOs.HiringManagerReports;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController, Route("api/manager/reports"), Authorize(Roles = nameof(UserRole.HiringManager))]
public sealed class HiringManagerReportsController(IHiringManagerReportService service) : ControllerBase
{
    private bool UserId(out int id) => int.TryParse(User.FindFirst("UserId")?.Value, out id);
    private static bool TryFilter(DateTime? from, DateTime? to, int? jobId, out HiringManagerReportFilter filter)
    {
        var end = (to ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
        var start = (from ?? end.AddMonths(-6)).Date;
        filter = new() { From = DateTime.SpecifyKind(start, DateTimeKind.Utc), To = DateTime.SpecifyKind(end, DateTimeKind.Utc), JobId = jobId };
        return start <= end && end - start <= TimeSpan.FromDays(366 * 2);
    }
    private async Task<ActionResult<HiringManagerReportDataDto>> Data(DateTime? from, DateTime? to, int? jobId, CancellationToken token)
    {
        if (!UserId(out var id)) return Unauthorized();
        if (!TryFilter(from, to, jobId, out var filter)) return BadRequest(new ProblemDetails { Status = 400, Title = "Invalid report date range" });
        var data = await service.GetAsync(id, filter, token);
        return data is null ? NotFound(new ProblemDetails { Status = 404, Title = "Job is outside your permitted scope" }) : data;
    }
    [HttpGet("summary")] public async Task<IActionResult> Summary(DateTime? from, DateTime? to, int? jobId, CancellationToken token) { var x = await Data(from, to, jobId, token); return x.Result ?? Ok(x.Value!.Summary); }
    [HttpGet("application-trends")] public async Task<IActionResult> Trends(DateTime? from, DateTime? to, int? jobId, CancellationToken token) { var x = await Data(from, to, jobId, token); return x.Result ?? Ok(x.Value!.Trends); }
    [HttpGet("hiring-funnel")] public async Task<IActionResult> Funnel(DateTime? from, DateTime? to, int? jobId, CancellationToken token) { var x = await Data(from, to, jobId, token); return x.Result ?? Ok(x.Value!.Funnel); }
    [HttpGet("job-performance")] public async Task<IActionResult> Jobs(DateTime? from, DateTime? to, int? jobId, CancellationToken token) { var x = await Data(from, to, jobId, token); return x.Result ?? Ok(x.Value!.JobPerformance); }
    [HttpGet("decision-outcomes")] public async Task<IActionResult> Outcomes(DateTime? from, DateTime? to, int? jobId, CancellationToken token) { var x = await Data(from, to, jobId, token); return x.Result ?? Ok(x.Value!.DecisionOutcomes); }
}
