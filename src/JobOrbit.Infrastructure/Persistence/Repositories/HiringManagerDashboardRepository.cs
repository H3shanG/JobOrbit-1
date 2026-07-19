using JobOrbit.Application.DTOs.Dashboard;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class HiringManagerDashboardRepository(JobOrbitDbContext db) : IHiringManagerDashboardRepository
{
    public async Task<HiringManagerDashboardStatsDto?> GetStatsAsync(int userId, CancellationToken token = default)
    {
        var scope = await db.HiringManagerProfiles.AsNoTracking().Where(x => x.UserId == userId && x.User.Role == UserRole.HiringManager && x.User.IsActive).Select(x => new { x.OrganizationId, x.DepartmentId }).SingleOrDefaultAsync(token);
        if (scope is null) return await db.Users.AnyAsync(x=>x.Id==userId&&x.Role==UserRole.HiringManager,token) ? new() : null;
        var applications=db.JobApplications.AsNoTracking().Where(x=>x.JobPosting.OrganizationId==scope.OrganizationId&&(!scope.DepartmentId.HasValue||x.JobPosting.DepartmentId==scope.DepartmentId));var today=DateTime.UtcNow.Date;var month=new DateTime(today.Year,today.Month,1,0,0,0,DateTimeKind.Utc);
        return new HiringManagerDashboardStatsDto { PendingReviews=await applications.CountAsync(x=>x.Status==ApplicationStatus.Shortlisted||x.Status==ApplicationStatus.Interviewing,token), TodaysInterviews=await applications.SelectMany(x=>x.Interviews).CountAsync(x=>x.ScheduledAt>=today&&x.ScheduledAt<today.AddDays(1)&&x.Status!=InterviewStatus.Cancelled,token), TeamFeedback=await applications.CountAsync(x=>x.CandidateEvaluations.Any(e=>e.EvaluatorUserId!=null)&&x.HiringDecision==null,token), HiredThisMonth=await applications.CountAsync(x=>x.HiringDecision!=null&&x.HiringDecision.Decision==ManagerHiringDecision.Hire&&x.HiringDecision.DecidedAt>=month,token) };
    }
}
