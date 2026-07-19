using JobOrbit.Application.DTOs.Dashboard;
using JobOrbit.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using JobOrbit.Domain.Enums;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class RecruiterDashboardRepository(JobOrbitDbContext db) : IRecruiterDashboardRepository
{
    public async Task<RecruiterDashboardStatsDto?> GetStatsAsync(int userId, CancellationToken token = default)
    {
        var profileId = await db.RecruiterProfiles.AsNoTracking().Where(x => x.UserId == userId).Select(x => (int?)x.Id).SingleOrDefaultAsync(token);
        if (!profileId.HasValue) return null;
        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);
        var jobs = db.JobPostings.AsNoTracking().Where(x => x.RecruiterProfileId == profileId.Value);
        var applications = db.JobApplications.AsNoTracking().Where(x => x.JobPosting.RecruiterProfileId == profileId.Value);
        return new RecruiterDashboardStatsDto
        {
            TotalJobs = await jobs.CountAsync(token),
            TotalApplications = await applications.CountAsync(token),
            TotalCandidates = await applications.Select(x => x.CandidateProfileId).Distinct().CountAsync(token),
            InterviewsThisMonth = await db.Interviews.AsNoTracking().CountAsync(x => x.JobApplication.JobPosting.RecruiterProfileId == profileId.Value && x.ScheduledAt >= start && x.ScheduledAt < end, token)
        };
    }

    public async Task<IReadOnlyList<RecruiterRecentApplicantDto>> GetRecentApplicantsAsync(int userId, CancellationToken token = default)
    {
        var rows = await db.JobApplications.AsNoTracking()
            .Where(x => x.JobPosting.RecruiterProfile.UserId == userId)
            .OrderByDescending(x => x.AppliedAt).ThenByDescending(x => x.Id).Take(5)
            .Select(x => new { x.Id, x.CandidateProfileId, x.CandidateProfile.User.FirstName, x.CandidateProfile.User.LastName, x.JobPostingId, x.JobPosting.Title, x.AppliedAt, x.Status })
            .ToListAsync(token);
        return rows.Select(x => new RecruiterRecentApplicantDto { ApplicationId=x.Id, CandidateId=x.CandidateProfileId, CandidateName=$"{x.FirstName} {x.LastName}".Trim(), JobId=x.JobPostingId, JobTitle=x.Title, AppliedOn=x.AppliedAt, Status=x.Status.ToString(), ProfileImageUrl=null }).ToList();
    }

    public async Task<IReadOnlyList<RecruiterUpcomingInterviewDto>> GetUpcomingInterviewsAsync(int userId, CancellationToken token = default)
    {
        var now = DateTime.UtcNow;
        var rows = await db.Interviews.AsNoTracking()
            .Where(x => x.JobApplication.JobPosting.RecruiterProfile.UserId == userId && x.ScheduledAt > now && x.Status != InterviewStatus.Cancelled && x.Status != InterviewStatus.Completed)
            .OrderBy(x => x.ScheduledAt).ThenBy(x => x.Id).Take(5)
            .Select(x => new { x.Id, x.JobApplicationId, x.JobApplication.CandidateProfileId, x.JobApplication.CandidateProfile.User.FirstName, x.JobApplication.CandidateProfile.User.LastName, x.JobApplication.JobPostingId, x.JobApplication.JobPosting.Title, x.ScheduledAt, x.DurationMinutes, x.Location, x.MeetingUrl, x.Status })
            .ToListAsync(token);
        return rows.Select(x => new RecruiterUpcomingInterviewDto { InterviewId=x.Id, ApplicationId=x.JobApplicationId, CandidateId=x.CandidateProfileId, CandidateName=$"{x.FirstName} {x.LastName}".Trim(), JobId=x.JobPostingId, JobTitle=x.Title, ScheduledAt=x.ScheduledAt, DurationMinutes=x.DurationMinutes, Location=x.Location, MeetingLink=x.MeetingUrl, Status=x.Status.ToString() }).ToList();
    }

    public async Task<RecruiterApplicationsOverviewDto> GetApplicationsOverviewAsync(int userId, CancellationToken token = default)
    {
        var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstMonth = currentMonth.AddMonths(-5); var end = currentMonth.AddMonths(1);
        var grouped = await db.JobApplications.AsNoTracking()
            .Where(x => x.JobPosting.RecruiterProfile.UserId == userId && x.AppliedAt >= firstMonth && x.AppliedAt < end)
            .GroupBy(x => new { x.AppliedAt.Year, x.AppliedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total=g.Count(), Shortlisted=g.Count(x=>x.Status==ApplicationStatus.Shortlisted), Rejected=g.Count(x=>x.Status==ApplicationStatus.Rejected), InterviewScheduled=g.Count(x=>x.Status==ApplicationStatus.Interviewing), Hired=g.Count(x=>x.Status==ApplicationStatus.Hired) })
            .ToListAsync(token);
        var lookup=grouped.ToDictionary(x=>(x.Year,x.Month));
        return new RecruiterApplicationsOverviewDto { Months=Enumerable.Range(0,6).Select(offset => { var month=firstMonth.AddMonths(offset); lookup.TryGetValue((month.Year,month.Month),out var value); return new RecruiterApplicationsOverviewMonthDto { Month=month.ToString("yyyy-MM"), Label=month.ToString("MMM",System.Globalization.CultureInfo.InvariantCulture), TotalApplications=value?.Total??0, Shortlisted=value?.Shortlisted??0, Rejected=value?.Rejected??0, InterviewScheduled=value?.InterviewScheduled??0, Hired=value?.Hired??0 }; }).ToList() };
    }
}
