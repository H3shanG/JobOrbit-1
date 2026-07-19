using System.Globalization;
using JobOrbit.Application.DTOs.RecruiterAnalytics;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class RecruiterAnalyticsRepository(JobOrbitDbContext db) : IRecruiterAnalyticsRepository
{
    public async Task<RecruiterAnalyticsDto> GetAsync(int userId, DateTime from, DateTime to, CancellationToken token = default)
    {
        var jobs = db.JobPostings.AsNoTracking()
            .Where(x => x.RecruiterProfile.UserId == userId && x.CreatedAt >= from && x.CreatedAt <= to);
        var applications = db.JobApplications.AsNoTracking()
            .Where(x => x.JobPosting.RecruiterProfile.UserId == userId && x.AppliedAt >= from && x.AppliedAt <= to);
        var interviews = db.Interviews.AsNoTracking()
            .Where(x => x.JobApplication.JobPosting.RecruiterProfile.UserId == userId && x.ScheduledAt >= from && x.ScheduledAt <= to);

        var totalJobs = await jobs.CountAsync(token);
        var publishedJobs = await jobs.CountAsync(x => x.Status == JobStatus.Published, token);
        var totalApplications = await applications.CountAsync(token);
        var shortlisted = await applications.CountAsync(x => x.Status == ApplicationStatus.Shortlisted, token);
        var offers = await applications.CountAsync(x => x.Status == ApplicationStatus.Offered, token);
        var hired = await applications.CountAsync(x => x.Status == ApplicationStatus.Hired, token);
        var rejected = await applications.CountAsync(x => x.Status == ApplicationStatus.Rejected, token);
        var interviewCount = await interviews.CountAsync(x => x.Status != InterviewStatus.Cancelled, token);

        var applicationGroups = await applications
            .GroupBy(x => new { x.AppliedAt.Year, x.AppliedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Applications = g.Count(),
                Shortlisted = g.Count(x => x.Status == ApplicationStatus.Shortlisted),
                Hired = g.Count(x => x.Status == ApplicationStatus.Hired)
            }).ToListAsync(token);
        var interviewGroups = await interviews.Where(x => x.Status != InterviewStatus.Cancelled)
            .GroupBy(x => new { x.ScheduledAt.Year, x.ScheduledAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() }).ToListAsync(token);
        var applicationLookup = applicationGroups.ToDictionary(x => (x.Year, x.Month));
        var interviewLookup = interviewGroups.ToDictionary(x => (x.Year, x.Month), x => x.Count);
        var firstMonth = new DateTime(from.Year, from.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonth = new DateTime(to.Year, to.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var trend = new List<RecruiterAnalyticsTrendDto>();
        for (var month = firstMonth; month <= lastMonth; month = month.AddMonths(1))
        {
            applicationLookup.TryGetValue((month.Year, month.Month), out var values);
            interviewLookup.TryGetValue((month.Year, month.Month), out var monthInterviews);
            trend.Add(new RecruiterAnalyticsTrendDto
            {
                Period = month.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                Label = month.ToString("MMM", CultureInfo.InvariantCulture),
                Applications = values?.Applications ?? 0,
                Shortlisted = values?.Shortlisted ?? 0,
                Interviews = monthInterviews,
                Hired = values?.Hired ?? 0
            });
        }

        var statusRows = await applications.GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync(token);
        var statusLookup = statusRows.ToDictionary(x => x.Status, x => x.Count);
        var byStatus = Enum.GetValues<ApplicationStatus>().Select(status =>
            new RecruiterApplicationStatusCountDto { Status = status.ToString(), Count = statusLookup.GetValueOrDefault(status) }).ToList();

        var topRows = await db.JobPostings.AsNoTracking()
            .Where(x => x.RecruiterProfile.UserId == userId)
            .Select(x => new
            {
                x.Id,
                x.Title,
                ApplicationCount = x.JobApplications.Count(a => a.AppliedAt >= from && a.AppliedAt <= to),
                ShortlistedCount = x.JobApplications.Count(a => a.AppliedAt >= from && a.AppliedAt <= to && a.Status == ApplicationStatus.Shortlisted),
                InterviewCount = x.JobApplications.SelectMany(a => a.Interviews).Count(i => i.ScheduledAt >= from && i.ScheduledAt <= to && i.Status != InterviewStatus.Cancelled),
                HiredCount = x.JobApplications.Count(a => a.AppliedAt >= from && a.AppliedAt <= to && a.Status == ApplicationStatus.Hired)
            }).OrderByDescending(x => x.ApplicationCount).ThenBy(x => x.Title).Take(5).ToListAsync(token);

        return new RecruiterAnalyticsDto
        {
            Summary = new RecruiterAnalyticsSummaryDto
            {
                TotalJobs = totalJobs,
                PublishedJobs = publishedJobs,
                TotalApplications = totalApplications,
                ShortlistedCandidates = shortlisted,
                InterviewsScheduled = interviewCount,
                OffersMade = offers,
                HiredCandidates = hired,
                RejectedApplications = rejected
            },
            ConversionRates = new RecruiterConversionRatesDto
            {
                ApplicationToShortlistRate = Rate(shortlisted, totalApplications),
                ShortlistToInterviewRate = Rate(interviewCount, shortlisted),
                InterviewToHireRate = Rate(hired, interviewCount)
            },
            ApplicationsTrend = trend,
            ApplicationsByStatus = byStatus,
            TopJobs = topRows.Select(x => new RecruiterTopJobDto
            {
                JobId = x.Id,
                JobTitle = x.Title,
                ApplicationCount = x.ApplicationCount,
                ShortlistedCount = x.ShortlistedCount,
                InterviewCount = x.InterviewCount,
                HiredCount = x.HiredCount
            }).ToList()
        };
    }

    private static decimal Rate(int numerator, int denominator) =>
        denominator == 0 ? 0 : Math.Round(numerator * 100m / denominator, 2);
}
