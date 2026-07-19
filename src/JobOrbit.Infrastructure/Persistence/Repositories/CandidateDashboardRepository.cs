using JobOrbit.Application.DTOs.Dashboard;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class CandidateDashboardRepository(JobOrbitDbContext dbContext)
    : ICandidateDashboardRepository
{
    public async Task<CandidateDashboardStatsDto> GetStatsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var applicationStats = await dbContext.JobApplications
            .AsNoTracking()
            .Where(application => application.CandidateProfile.UserId == userId)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                JobsApplied = group.Count(),
                Shortlisted = group.Count(application =>
                    application.Status == ApplicationStatus.Shortlisted),
                Pending = group.Count(application =>
                    application.Status == ApplicationStatus.Submitted ||
                    application.Status == ApplicationStatus.UnderReview)
            })
            .SingleOrDefaultAsync(cancellationToken);

        var scheduledInterviews = await dbContext.Interviews
            .AsNoTracking()
            .CountAsync(
                interview =>
                    interview.JobApplication.CandidateProfile.UserId == userId &&
                    interview.Status == InterviewStatus.Scheduled,
                cancellationToken);

        return new CandidateDashboardStatsDto
        {
            JobsApplied = applicationStats?.JobsApplied ?? 0,
            Interviews = scheduledInterviews,
            Shortlisted = applicationStats?.Shortlisted ?? 0,
            Pending = applicationStats?.Pending ?? 0
        };
    }

    public async Task<IReadOnlyList<RecentApplicationDto>> GetRecentApplicationsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var applications = await dbContext.JobApplications
            .AsNoTracking()
            .Where(application => application.CandidateProfile.UserId == userId)
            .OrderByDescending(application => application.AppliedAt)
            .ThenByDescending(application => application.Id)
            .Take(5)
            .Select(application => new
            {
                ApplicationId = application.Id,
                JobId = application.JobPostingId,
                JobTitle = application.JobPosting.Title,
                CompanyName = application.JobPosting.Organization.Name,
                application.Status,
                AppliedOn = application.AppliedAt
            })
            .ToListAsync(cancellationToken);

        return applications.Select(application => new RecentApplicationDto
        {
            ApplicationId = application.ApplicationId,
            JobId = application.JobId,
            JobTitle = application.JobTitle,
            CompanyName = application.CompanyName,
            Status = application.Status.ToString(),
            AppliedOn = application.AppliedOn
        }).ToList();
    }

    public async Task<IReadOnlyList<RecommendedJobDto>> GetRecommendedJobsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await dbContext.JobPostings
            .AsNoTracking()
            .Where(job =>
                job.Status == JobStatus.Published &&
                (!job.ClosingAt.HasValue || job.ClosingAt > now) &&
                !job.JobApplications.Any(application =>
                    application.CandidateProfile.UserId == userId))
            .OrderByDescending(job => job.PublishedAt ?? job.CreatedAt)
            .ThenByDescending(job => job.Id)
            .Take(3)
            .Select(job => new RecommendedJobDto
            {
                JobId = job.Id,
                Title = job.Title,
                CompanyName = job.Organization.Name,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                PostedOn = job.PublishedAt ?? job.CreatedAt,
                ClosingDate = job.ClosingAt,
                Skills = job.JobSkills
                    .OrderByDescending(jobSkill => jobSkill.IsRequired)
                    .ThenBy(jobSkill => jobSkill.Skill.Name)
                    .Select(jobSkill => jobSkill.Skill.Name)
                    .ToList(),
                MatchScore = null
            })
            .ToListAsync(cancellationToken);
    }
}
